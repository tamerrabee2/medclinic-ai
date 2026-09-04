using Microsoft.AspNetCore.Identity;

namespace MedClinic.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClinicMember> ClinicMemberships { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
