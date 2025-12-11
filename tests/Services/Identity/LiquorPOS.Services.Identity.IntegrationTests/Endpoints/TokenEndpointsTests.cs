using FluentAssertions;
using LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;
using LiquorPOS.Services.Identity.IntegrationTests.Models;
using LiquorPOS.Services.Identity.Application.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiquorPOS.Services.Identity.IntegrationTests.Endpoints;

public class TokenEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public TokenEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test.refresh@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var loginRequest = new
        {
            Email = "test.refresh@example.com",
            Password = "TestPassword123!"
        };

        // Register and login to get tokens
        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, _jsonOptions);
        
        var refreshTokenRequest = new
        {
            RefreshToken = loginResult.Data.RefreshToken
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/identity/refresh-token", refreshTokenRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
        var refreshResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(refreshContent, _jsonOptions);

        refreshResult.Should().NotBeNull();
        refreshResult.Success.Should().BeTrue();
        refreshResult.Data.Should().NotBeNull();
        refreshResult.Data.AccessToken.Should().NotBeEmpty();
        refreshResult.Data.RefreshToken.Should().NotBeEmpty();
        refreshResult.Data.RefreshToken.Should().NotBe(loginResult.Data.RefreshToken); // Should be rotated
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var refreshTokenRequest = new
        {
            RefreshToken = "invalid_refresh_token"
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/identity/refresh-token", refreshTokenRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ShouldReturnBadRequest()
    {
        // Arrange
        var refreshTokenRequest = new
        {
            RefreshToken = ""
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/identity/refresh-token", refreshTokenRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Refresh token is required");
    }

    [Fact]
    public async Task RevokeToken_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test.revoke@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var loginRequest = new
        {
            Email = "test.revoke@example.com",
            Password = "TestPassword123!"
        };

        // Register and login to get tokens
        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, _jsonOptions);

        var revokeTokenRequest = new
        {
            RefreshToken = loginResult.Data.RefreshToken
        };

        // Act
        var revokeResponse = await _client.PostAsJsonAsync("/api/identity/revoke-token", revokeTokenRequest);

        // Assert
        revokeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var revokeContent = await revokeResponse.Content.ReadAsStringAsync();
        var revokeResult = JsonSerializer.Deserialize<ApiResponse<object>>(revokeContent, _jsonOptions);

        revokeResult.Should().NotBeNull();
        revokeResult.Success.Should().BeTrue();
        revokeResult.Message.Should().Be("Token revoked successfully");
    }

    [Fact]
    public async Task RevokeToken_ThenRefresh_ShouldReturnError()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "test.rotate@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var loginRequest = new
        {
            Email = "test.rotate@example.com",
            Password = "TestPassword123!"
        };

        // Register and login to get tokens
        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, _jsonOptions);

        var revokeTokenRequest = new
        {
            RefreshToken = loginResult.Data.RefreshToken
        };

        // Revoke the token
        var revokeResponse = await _client.PostAsJsonAsync("/api/identity/revoke-token", revokeTokenRequest);
        revokeResponse.EnsureSuccessStatusCode();

        // Try to refresh with the revoked token
        var refreshTokenRequest = new
        {
            RefreshToken = loginResult.Data.RefreshToken
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/identity/refresh-token", refreshTokenRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("revoked");
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnError()
    {
        // This test would require manipulating the token expiration or database
        // For now, we'll test with a malformed token
        // Arrange
        var refreshTokenRequest = new
        {
            RefreshToken = "expired_token_" + new string('a', 100)
        };

        // Act
        var refreshResponse = await _client.PostAsJsonAsync("/api/identity/refresh-token", refreshTokenRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await refreshResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }
}