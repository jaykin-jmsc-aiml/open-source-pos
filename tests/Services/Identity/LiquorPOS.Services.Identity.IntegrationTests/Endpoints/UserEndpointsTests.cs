using FluentAssertions;
using LiquorPOS.Services.Identity.IntegrationTests.Infrastructure;
using LiquorPOS.Services.Identity.IntegrationTests.Models;
using LiquorPOS.Services.Identity.Application.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace LiquorPOS.Services.Identity.IntegrationTests.Endpoints;

public class UserEndpointsTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;
    private readonly IdentityApiFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserEndpointsTests(IdentityApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/identity/users");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithValidAuth_ShouldReturnUsers()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.GetAsync("/api/identity/users");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<UserListDto>>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsers_WithSearchTerm_ShouldReturnFilteredResults()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.GetAsync("/api/identity/users?searchTerm=admin");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<UserListDto>>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsers_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.GetAsync("/api/identity/users?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<UserListDto>>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetUserById_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/identity/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_WithValidAuth_NonExistentUser_ShouldReturnNotFound()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        var nonExistentUserId = Guid.NewGuid();

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.GetAsync($"/api/identity/users/{nonExistentUserId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserById_WithValidAuth_ExistentUser_ShouldReturnUser()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // First, register a user to get their ID
        var registerRequest = new
        {
            Email = "test.user@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(registerContent, _jsonOptions);
        
        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.GetAsync($"/api/identity/users/{registerResult.Data.UserId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("test.user@example.com");
        result.Data.FirstName.Should().Be("Test");
        result.Data.LastName.Should().Be("User");
    }

    [Fact]
    public async Task AssignUserRoles_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rolesRequest = new[] { "Admin", "User" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/identity/users/{userId}/roles", rolesRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignUserRoles_WithValidAuth_NonExistentUser_ShouldReturnNotFound()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        var nonExistentUserId = Guid.NewGuid();
        var rolesRequest = new[] { "Admin", "User" };

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.PostAsJsonAsync($"/api/identity/users/{nonExistentUserId}/roles", rolesRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignUserRoles_WithValidAuth_ExistentUser_ShouldReturnSuccess()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // First, register a user to get their ID
        var registerRequest = new
        {
            Email = "test.assign@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(registerContent, _jsonOptions);

        var rolesRequest = new[] { "Manager", "User" };

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.PostAsJsonAsync($"/api/identity/users/{registerResult.Data.UserId}/roles", rolesRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Roles assigned successfully");
    }

    [Fact]
    public async Task AssignUserRoles_WithInvalidRoles_ShouldReturnBadRequest()
    {
        // Arrange - Create admin user and get token
        var adminToken = await CreateAdminUserAndGetToken();

        // First, register a user to get their ID
        var registerRequest = new
        {
            Email = "test.invalid@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "TestPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "User" }
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(registerContent, _jsonOptions);

        var rolesRequest = new[] { "InvalidRole" };

        // Act
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        
        var response = await _client.PostAsJsonAsync($"/api/identity/users/{registerResult.Data.UserId}/roles", rolesRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("detail").GetString().Should().Contain("Invalid roles");
    }

    private async Task<string> CreateAdminUserAndGetToken()
    {
        var uniqueEmail = $"admin-{Guid.NewGuid()}@example.com";
        var adminRequest = new
        {
            Email = uniqueEmail,
            FirstName = "Admin",
            LastName = "User",
            Password = "AdminPassword123!",
            PhoneNumber = "+1234567890",
            Roles = new[] { "Admin" }
        };

        var adminResponse = await _client.PostAsJsonAsync("/api/identity/register", adminRequest);
        adminResponse.EnsureSuccessStatusCode();

        var loginRequest = new
        {
            Email = uniqueEmail,
            Password = "AdminPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(loginContent, _jsonOptions);

        return loginResult.Data.AccessToken;
    }
}