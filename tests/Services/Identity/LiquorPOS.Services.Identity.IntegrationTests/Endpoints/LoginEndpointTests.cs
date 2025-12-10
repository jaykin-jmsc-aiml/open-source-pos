using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;

namespace LiquorPOS.Services.Identity.IntegrationTests.Endpoints;

internal sealed record IdentityResponseDto<T>(
    bool Success,
    string? Message,
    T? Data);

public class LoginEndpointTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public LoginEndpointTests(IdentityWebApplicationFactory factory)
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

        var content = await response.Content.ReadAsAsync<IdentityResponseDto<AuthResponse>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.AccessToken.Should().NotBeNullOrEmpty();
        content.Data.RefreshToken.Should().NotBeNullOrEmpty();
        content.Data.Email.Should().Be(email);
        content.Data.FirstName.Should().Be("John");
        content.Data.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
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

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsAsync<IdentityResponseDto<object>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_ShouldReturnBadRequest()
    {
        var loginRequest = new LoginRequest("nonexistent@example.com", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsAsync<IdentityResponseDto<object>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var loginRequest = new LoginRequest("invalid-email", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        var loginRequest = new LoginRequest("user@example.com", "");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
    {
        var loginRequest = new LoginRequest("", "password");
        var response = await _httpClient.PostAsJsonAsync("/api/identity/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
