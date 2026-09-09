using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MedClinic.Tests.Integration.Fixtures;
using Xunit;

namespace MedClinic.Tests.Integration.Security;

public class RateLimitingTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_RateLimiter_Returns429_WhenLimitExceeded()
    {
        // Policy: auth-forgot-password-policy allows 3 requests / min per IP
        var payload = new { email = "rate_limit_test@medclinic.test" };

        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 5; i++)
        {
            var res = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", payload);
            responses.Add(res);
        }

        // At least one subsequent request must be throttled with HTTP 429
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }
}
