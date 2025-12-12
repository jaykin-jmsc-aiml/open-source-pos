using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;
using LiquorPOS.Services.Identity.IntegrationTests.Models;
using System.Text.Json;

namespace LiquorPOS.Services.Identity.IntegrationTests.Endpoints;

public class RegisterEndpointTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RegisterEndpointTests(IdentityApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnSuccess()
    {
        var request = new RegisterRequest(
            "newuser@example.com",
            "John",
            "Doe",
            "SecurePass123");

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, _jsonOptions);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.Email.Should().Be("newuser@example.com");
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            "invalid-email",
            "John",
            "Doe",
            "SecurePass123");

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "John",
            "Doe",
            "weak");

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithEmptyFirstName_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            "user@example.com",
            "",
            "Doe",
            "SecurePass123");

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            "duplicate@example.com",
            "John",
            "Doe",
            "SecurePass123");

        var response1 = await _httpClient.PostAsJsonAsync("/api/identity/register", request);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var response2 = await _httpClient.PostAsJsonAsync("/api/identity/register", request);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response2.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("already registered");
    }

    [Fact]
    public async Task Register_WithValidPhoneNumber_ShouldSucceed()
    {
        var request = new RegisterRequest(
            "userphone@example.com",
            "John",
            "Doe",
            "SecurePass123",
            "+1234567890");

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, _jsonOptions);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithCustomRoles_ShouldSucceed()
    {
        var request = new RegisterRequest(
            "customrole@example.com",
            "John",
            "Doe",
            "SecurePass123",
            null,
            new[] { "Manager" });

        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, _jsonOptions);
        result.Success.Should().BeTrue();
    }
}
