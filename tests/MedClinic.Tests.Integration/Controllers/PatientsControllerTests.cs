using FluentAssertions;
using MedClinic.Tests.Integration.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace MedClinic.Tests.Integration.Controllers;

public class PatientsControllerTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    public PatientsControllerTests(WebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPatients_Returns401_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/v1/patients");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePatient_Returns401_WhenNotAuthenticated()
    {
        var body = new
        {
            firstName = "Test", lastName = "Patient",
            phone = "0501234567", gender = "Male"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/patients", body);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
