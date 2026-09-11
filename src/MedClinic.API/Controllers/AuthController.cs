using MedClinic.Application.Interfaces;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Identity;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace MedClinic.API.Controllers;

[EnableRateLimiting("auth-policy")]
public class AuthController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;
    private readonly MedClinic.Application.Common.Interfaces.IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ApplicationDbContext context,
        MedClinic.Application.Common.Interfaces.IEmailService emailService,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest("Email already registered.");

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            PreferredLanguage = request.PreferredLanguage ?? "en"
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

        return Created(new { UserId = user.Id, Email = user.Email }, "Registration successful.");
    }

    /// <summary>Login and get JWT tokens</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login-policy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut) return Unauthorized("Account locked. Try again later.");
            return Unauthorized("Invalid credentials.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

        // Revoke old refresh tokens for this user (single session per user)
        var oldTokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var old in oldTokens)
        {
            old.IsRevoked = true;
            old.RevokedAt = DateTime.UtcNow;
        }

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        // Get user's clinics
        var clinics = await _context.ClinicMembers
            .Where(cm => cm.UserId == user.Id && !cm.IsDeleted)
            .Include(cm => cm.Clinic)
            .Select(cm => new { cm.Clinic.Id, cm.Clinic.Name, cm.Clinic.Slug, cm.Role })
            .ToListAsync(ct);

        return Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PreferredLanguage = user.PreferredLanguage,
                AvatarUrl = user.AvatarUrl,
                Roles = [.. roles],
                Permissions = PermissionClaimsFactory.GetPermissionsForRoles(roles).ToList()
            },
            Clinics = clinics.Select(c => new ClinicBriefDto(c.Id, c.Name, c.Slug, c.Role)).ToList()
        });
    }

    /// <summary>Switch active clinic — returns new token with clinic_id claim</summary>
    [HttpPost("switch-clinic")]
    [Authorize]
    public async Task<IActionResult> SwitchClinic([FromBody] SwitchClinicRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;

        var isMember = await _context.ClinicMembers
            .AnyAsync(cm => cm.ClinicId == request.ClinicId && cm.UserId == userId && !cm.IsDeleted, ct);

        if (!isMember)
            return Unauthorized("You are not a member of this clinic.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessTokenWithClinic(user, roles, request.ClinicId);

        return Success(new { AccessToken = accessToken, ClinicId = request.ClinicId });
    }

    /// <summary>Refresh access token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh-policy")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked, ct);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(ct);

        return Success(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
    }

    /// <summary>Forgot password — generate reset token and dispatch link</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-forgot-password-policy")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        // Always return generic 200 message to prevent email enumeration and token leakage
        if (user != null && !string.IsNullOrWhiteSpace(user.Email))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = _config["App:FrontendUrl"] ?? "http://localhost:3000";
            var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, ct);
        }

        return Success<object>(null!, "If this email exists, a reset instructions link has been sent.");
    }

    /// <summary>Reset password with token</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) return BadRequest("Invalid request.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Revoke all refresh tokens on password reset
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var t in tokens) { t.IsRevoked = true; t.RevokedAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync(ct);

        return Success<object>(null!, "Password reset successfully.");
    }

    /// <summary>Change password (authenticated)</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user == null) return NotFound("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

        return Success<object>(null!, "Password changed successfully.");
    }

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user == null) return NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var clinics = await _context.ClinicMembers
            .Where(cm => cm.UserId == user.Id && !cm.IsDeleted)
            .Include(cm => cm.Clinic)
            .Select(cm => new ClinicBriefDto(cm.Clinic.Id, cm.Clinic.Name, cm.Clinic.Slug, cm.Role))
            .ToListAsync(ct);

        return Success(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            PreferredLanguage = user.PreferredLanguage,
            AvatarUrl = user.AvatarUrl,
            Roles = [.. roles],
            Permissions = PermissionClaimsFactory.GetPermissionsForRoles(roles).ToList(),
            Clinics = clinics
        });
    }

    /// <summary>Logout — revoke refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == CurrentUserId, ct);

        if (token != null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        return Success<object>(null!, "Logged out successfully.");
    }
}

// ---- DTOs ----
public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string? PreferredLanguage);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record SwitchClinicRequest(Guid ClinicId);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ClinicBriefDto(Guid Id, string Name, string Slug, string Role);

public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = null!;
    public List<ClinicBriefDto> Clinics { get; init; } = [];
}

public record UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PreferredLanguage { get; init; } = "en";
    public string? AvatarUrl { get; init; }
    public List<string> Roles { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
}

public record UserProfileDto : UserDto
{
    public List<ClinicBriefDto> Clinics { get; init; } = [];
}
