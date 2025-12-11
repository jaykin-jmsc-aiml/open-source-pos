using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Queries;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Queries;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<LiquorPOSIdentityDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<GetUsersQueryHandler>> _loggerMock = new();
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new GetUsersQueryHandler(
            _userManagerMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                FirstName = "User",
                LastName = "One",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                FirstName = "User",
                LastName = "Two",
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        }.AsQueryable();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users);

        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Users retrieved successfully");
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterCorrectly()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "jane@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        }.AsQueryable();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users);

        var query = new GetUsersQuery(1, 10, "john", null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Email = "active@example.com",
                FirstName = "Active",
                LastName = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Email = "inactive@example.com",
                FirstName = "Inactive",
                LastName = "User",
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        }.AsQueryable();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users);

        var query = new GetUsersQuery(1, 10, null, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Email.Should().Be("active@example.com");
    }

    [Fact]
    public async Task Handle_WithEmptyUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var users = new List<ApplicationUser>().AsQueryable();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users);

        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var users = new List<ApplicationUser>
        {
            new() { Id = Guid.NewGuid(), Email = "user1@example.com", FirstName = "User", LastName = "One", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "user2@example.com", FirstName = "User", LastName = "Two", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "user3@example.com", FirstName = "User", LastName = "Three", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "user4@example.com", FirstName = "User", LastName = "Four", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Email = "user5@example.com", FirstName = "User", LastName = "Five", IsActive = true, CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

        _userManagerMock.Setup(x => x.Users)
            .Returns(users);

        var query = new GetUsersQuery(2, 2, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.Items.First().Email.Should().Be("user3@example.com");
        result.Data.PageNumber.Should().Be(2);
        result.Data.PageSize.Should().Be(2);
        result.Data.TotalCount.Should().Be(5);
        result.Data.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithException_ShouldReturnFailure()
    {
        // Arrange
        _userManagerMock.Setup(x => x.Users)
            .Throws(new Exception("Database error"));

        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving users");
        result.Data.Should().BeNull();
    }
}