using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MedClinic.Domain.Entities;
using MedClinic.Infrastructure.Persistence;
using MedClinic.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedClinic.Tests.Integration.Security;

public class PasswordResetTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public PasswordResetTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_Returns200_AndDoesNotLeakTokenInResponse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"reset_test_{Guid.NewGuid():N}@medclinic.test";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            FirstName = "Reset",
            LastName = "User",
            IsActive = true
        };
        var createResult = await userManager.CreateAsync(user, "StrongPass@123456");
        createResult.Succeeded.Should().BeTrue();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();

        // Ensure token is NOT leaked in the payload
        json.ToLowerInvariant().Should().NotContain("resettoken");
        json.ToLowerInvariant().Should().NotContain("token\":\"");
        json.Should().Contain("reset instructions link has been sent");
    }

    [Fact]
    public async Task ForgotPassword_NonExistingUser_Returns200_GenericMessage()
    {
        // Arrange
        var nonExistentEmail = $"ghost_{Guid.NewGuid():N}@doesnotexist.test";

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = nonExistentEmail });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("reset instructions link has been sent");
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            email = "someuser@test.com",
            token = "INVALID_BOGUS_TOKEN_12345",
            newPassword = "NewPassword@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
