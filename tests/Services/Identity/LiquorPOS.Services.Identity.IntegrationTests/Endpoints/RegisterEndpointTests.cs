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

public class RegisterEndpointTests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public RegisterEndpointTests(IdentityWebApplicationFactory factory)
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

        var content = await response.Content.ReadAsAsync<IdentityResponseDto<AuthResponse>>();
        content.Should().NotBeNull();
        content!.Success.Should().BeTrue();
        content.Data.Should().NotBeNull();
        content.Data!.AccessToken.Should().NotBeNullOrEmpty();
        content.Data.RefreshToken.Should().NotBeNullOrEmpty();
        content.Data.Email.Should().Be("newuser@example.com");
        content.Data.FirstName.Should().Be("John");
        content.Data.LastName.Should().Be("Doe");
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

        var content = await response2.Content.ReadAsAsync<IdentityResponseDto<object>>();
        content!.Success.Should().BeFalse();
        content.Message.Should().Contain("already registered");
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
        var content = await response.Content.ReadAsAsync<IdentityResponseDto<AuthResponse>>();
        content!.Success.Should().BeTrue();
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
        var content = await response.Content.ReadAsAsync<IdentityResponseDto<AuthResponse>>();
        content!.Success.Should().BeTrue();
    }
}
