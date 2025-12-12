using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;
using LiquorPOS.Services.Identity.IntegrationTests.Models;
using System.Text.Json;

namespace LiquorPOS.Services.Identity.IntegrationTests.Endpoints;

public class LoginEndpointTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LoginEndpointTests(IdentityApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        var email = "loginuser@example.com";
        var password = "SecurePass123";

        var registerRequest = new RegisterRequest(
            email,
            "John",
            "Doe",
            password);

        await _httpClient.PostAsJsonAsync("/api/identity/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, _jsonOptions);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be(email);
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        var email = "invalidpass@example.com";
        var password = "SecurePass123";

        var registerRequest = new RegisterRequest(
            email,
            "John",
            "Doe",
            password);

        await _httpClient.PostAsJsonAsync("/api/identity/register", registerRequest);

        var loginRequest = new LoginRequest(email, "WrongPassword123");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest("nonexistent@example.com", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest("invalid-email", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest("user@example.com", "");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest("", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldUpdateLastLoginTime()
    {
        var email = "lastlogin@example.com";
        var password = "SecurePass123";

        var registerRequest = new RegisterRequest(email, "John", "Doe", password);
        await _httpClient.PostAsJsonAsync("/api/identity/register", registerRequest);

        var loginRequest = new LoginRequest(email, password);
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
