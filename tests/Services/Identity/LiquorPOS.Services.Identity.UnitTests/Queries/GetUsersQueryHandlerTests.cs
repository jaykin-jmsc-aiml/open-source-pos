using FluentAssertions;
using LiquorPOS.Services.Identity.Application.Dtos;
using LiquorPOS.Services.Identity.Application.Queries;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers;
using LiquorPOS.Services.Identity.UnitTests.TestHelpers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LiquorPOS.Services.Identity.UnitTests.Queries;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<ILogger<GetUsersQueryHandler>> _loggerMock = new();

    private (UserManager<ApplicationUser>, LiquorPOSIdentityDbContext, GetUsersQueryHandler) CreateHandler()
    {
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, LiquorPOSIdentityDbContext, Guid>(dbContext);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null);
        var handler = new GetUsersQueryHandler(userManager, dbContext, _loggerMock.Object);
        return (userManager, dbContext, handler);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSuccess()
    {
        // Arrange
        var (userManager, dbContext, handler) = CreateHandler();
        
        var user1 = new UserBuilder()
            .WithEmail("user1@example.com")
            .WithFirstName("User")
            .WithLastName("One")
            .WithIsActive(true)
            .Build();
        
        var user2 = new UserBuilder()
            .WithEmail("user2@example.com")
            .WithFirstName("User")
            .WithLastName("Two")
            .WithIsActive(false)
            .Build();

        await userManager.CreateAsync(user1);
        await userManager.CreateAsync(user2);

        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue($"because query should succeed, but got message: {result.Message}");
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
        var (userManager, dbContext, handler) = CreateHandler();
        
        var user1 = new UserBuilder()
            .WithEmail("john@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithIsActive(true)
            .Build();
        
        var user2 = new UserBuilder()
            .WithEmail("jane@example.com")
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .WithIsActive(true)
            .Build();

        await userManager.CreateAsync(user1);
        await userManager.CreateAsync(user2);

        var query = new GetUsersQuery(1, 10, "john", null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var (userManager, dbContext, handler) = CreateHandler();
        
        var user1 = new UserBuilder()
            .WithEmail("active@example.com")
            .WithFirstName("Active")
            .WithLastName("User")
            .WithIsActive(true)
            .Build();
        
        var user2 = new UserBuilder()
            .WithEmail("inactive@example.com")
            .WithFirstName("Inactive")
            .WithLastName("User")
            .WithIsActive(false)
            .Build();

        await userManager.CreateAsync(user1);
        await userManager.CreateAsync(user2);

        var query = new GetUsersQuery(1, 10, null, true);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Email.Should().Be("active@example.com");
    }

    [Fact]
    public async Task Handle_WithEmptyUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var (userManager, dbContext, handler) = CreateHandler();
        
        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var (userManager, dbContext, handler) = CreateHandler();
        
        await userManager.CreateAsync(new UserBuilder().WithEmail("user1@example.com").WithFirstName("User").WithLastName("One").WithIsActive(true).Build());
        await userManager.CreateAsync(new UserBuilder().WithEmail("user2@example.com").WithFirstName("User").WithLastName("Two").WithIsActive(true).Build());
        await userManager.CreateAsync(new UserBuilder().WithEmail("user3@example.com").WithFirstName("User").WithLastName("Three").WithIsActive(true).Build());
        await userManager.CreateAsync(new UserBuilder().WithEmail("user4@example.com").WithFirstName("User").WithLastName("Four").WithIsActive(true).Build());
        await userManager.CreateAsync(new UserBuilder().WithEmail("user5@example.com").WithFirstName("User").WithLastName("Five").WithIsActive(true).Build());

        var query = new GetUsersQuery(2, 2, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

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
        // Arrange - Use a disposed context to trigger an exception
        var dbContext = InMemoryIdentityDbContextFactory.CreateDbContext();
        var userStore = new UserStore<ApplicationUser, IdentityRole<Guid>, LiquorPOSIdentityDbContext, Guid>(dbContext);
        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null);
        var handler = new GetUsersQueryHandler(userManager, dbContext, _loggerMock.Object);
        
        // Dispose the context to cause an exception when accessing it
        dbContext.Dispose();

        var query = new GetUsersQuery(1, 10, null, null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while retrieving users");
        result.Data.Should().BeNull();
    }
}