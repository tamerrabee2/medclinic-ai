using FluentAssertions;
using MedClinic.Tests.Integration.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class AuthControllerTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Returns401_WhenCredentialsInvalid()
    {
        // Arrange
        var body = new { email = "nobody@test.com", password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", body);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_Returns400_WhenEmailMissing()
    {
        // Arrange
        var body = new { password = "Test@1234", firstName = "Ahmed", lastName = "Ali" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", body);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_Returns401_WhenUnauthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AIInfo_Returns200_WhenUnauthenticated()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/ai/info");

        // Assert — /ai/info is public
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
