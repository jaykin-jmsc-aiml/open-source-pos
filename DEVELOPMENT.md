# Development Guide

## Quick Start

### Prerequisites
- .NET 8 SDK installed
- Node.js 20+ installed
- Docker and Docker Compose installed

### First Time Setup

```bash
# Clone and navigate to the repository
cd /path/to/LiquorPOS

# Restore all NuGet packages
dotnet restore

# Install frontend dependencies
cd src/Frontend
npm install
cd ../..

# Build the entire solution
dotnet build

# Run tests
dotnet test

# Run frontend tests
cd src/Frontend
npm run test
```

### Running Services Locally

#### Option 1: Docker Compose (Recommended)
```bash
# Build and start all services
docker compose up --build

# Run in background
docker compose up -d

# View logs
docker compose logs -f

# Stop all services
docker compose down
```

#### Option 2: Individual Services
```bash
# Terminal 1 - Start Infrastructure
docker compose up sqlserver rabbitmq

# Terminal 2 - Start Identity Service
cd src/Services/Identity/LiquorPOS.Services.Identity.Api
dotnet run

# Terminal 3 - Start Catalog Service
cd src/Services/Catalog/LiquorPOS.Services.Catalog.Api
dotnet run

# Terminal 4 - Start API Gateway
cd src/ApiGateway/LiquorPOS.ApiGateway
dotnet run

# Terminal 5 - Start Frontend
cd src/Frontend
npm run dev
```

## Project Structure

### BuildingBlocks

The `BuildingBlocks` project contains shared abstractions used across all services:

```csharp
// Result Pattern
var result = Result.Success();
var resultWithValue = Result.Success(myData);
var failure = Result.Failure("Error message");

// Domain Entities
public class Product : Entity<Guid>
{
    public string Name { get; set; }
    // ...
}

// Auditable Entities
public class Order : AuditableEntity
{
    // Automatically includes: CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy
}

// Exceptions
throw new DomainException("Business rule violated");
throw new NotFoundException("Product", productId);
```

### Adding a New Service

If you need to add a new service:

1. Create the project structure:
```bash
SERVICE="NewService"
dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Domain" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain"
dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Application" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application"
dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Infrastructure" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure"
dotnet new webapi -n "LiquorPOS.Services.$SERVICE.Api" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api"
dotnet new xunit -n "LiquorPOS.Services.$SERVICE.UnitTests" -o "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests"

# Add to solution
dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain"
dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application"
dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure"
dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api"
dotnet sln add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests"
```

2. Add required packages (following the same pattern as existing services)

3. Add project references

4. Create Dockerfile

5. Add to docker-compose.yml

### Adding a New Feature to a Service

Following Clean Architecture:

#### 1. Domain Layer
Create domain entities:
```csharp
// src/Services/Catalog/LiquorPOS.Services.Catalog.Domain/Entities/Product.cs
public class Product : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // ...
}
```

#### 2. Application Layer
Create DTOs and MediatR commands/queries:
```csharp
// src/Services/Catalog/LiquorPOS.Services.Catalog.Application/Products/Commands/CreateProduct/CreateProductCommand.cs
public record CreateProductCommand(string Name, decimal Price) : IRequest<Result<Guid>>;

// CreateProductCommandHandler.cs
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}

// CreateProductCommandValidator.cs
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

#### 3. Infrastructure Layer
Implement data access:
```csharp
// src/Services/Catalog/LiquorPOS.Services.Catalog.Infrastructure/Data/CatalogDbContext.cs
public class CatalogDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().ToTable("Products");
    }
}
```

#### 4. API Layer
Create endpoints:
```csharp
// src/Services/Catalog/LiquorPOS.Services.Catalog.Api/Controllers/ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }
}
```

### Database Migrations

Each service manages its own database:

```bash
# Navigate to the API project
cd src/Services/Catalog/LiquorPOS.Services.Catalog.Api

# Add migration
dotnet ef migrations add InitialCreate -p ../LiquorPOS.Services.Catalog.Infrastructure

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove -p ../LiquorPOS.Services.Catalog.Infrastructure
```

## Testing

### Backend Unit Tests

```bash
# Run all tests
dotnet test

# Run tests for specific service
dotnet test tests/Services/Catalog/LiquorPOS.Services.Catalog.UnitTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Example test:
```csharp
public class ProductTests
{
    [Fact]
    public void Product_Should_Have_Valid_Name()
    {
        // Arrange
        var product = new Product { Name = "Test Product" };
        
        // Act & Assert
        product.Name.Should().NotBeEmpty();
    }
}
```

### Frontend Tests

```bash
cd src/Frontend

# Run tests
npm run test

# Run tests in watch mode
npm run test -- --watch

# Run tests with coverage
npm run test -- --coverage
```

## API Documentation

Each service exposes Swagger documentation in development mode:

- Identity: http://localhost:5001/swagger
- Catalog: http://localhost:5002/swagger
- Inventory: http://localhost:5003/swagger
- Sales: http://localhost:5004/swagger
- Loyalty: http://localhost:5005/swagger
- Reporting: http://localhost:5006/swagger
- Configuration: http://localhost:5007/swagger
- Gateway: http://localhost:5000

## Debugging

### Visual Studio Code

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (Identity API)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Services/Identity/LiquorPOS.Services.Identity.Api/bin/Debug/net8.0/LiquorPOS.Services.Identity.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Services/Identity/LiquorPOS.Services.Identity.Api",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### Visual Studio

Open `LiquorPOS.sln` and set the desired API project as the startup project.

## Common Issues

### EF Core Version Mismatch
**Problem**: Build fails with EF Core compatibility errors  
**Solution**: Ensure you're using EF Core 8.0.11, not 10.0

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
```

### Port Already in Use
**Problem**: Service fails to start due to port conflict  
**Solution**: Change the port in `launchSettings.json` or stop the conflicting process

### Docker Build Fails
**Problem**: Dockerfile can't find projects  
**Solution**: Ensure you're building from the repository root with proper context

## Performance Tips

1. **Use `dotnet watch`** for hot reload during development
2. **Enable response caching** for frequently accessed data
3. **Use async/await** consistently
4. **Implement database indexes** for common queries
5. **Use pagination** for list endpoints

## Code Quality

### Linting and Formatting

```bash
# Format all code
dotnet format

# Check for formatting issues
dotnet format --verify-no-changes
```

### Static Analysis

Consider adding:
- StyleCop.Analyzers
- SonarAnalyzer.CSharp
- Microsoft.CodeAnalysis.NetAnalyzers

## Contributing

1. Create a feature branch from `main`
2. Make your changes following the existing patterns
3. Add tests for new functionality
4. Ensure all tests pass: `dotnet test`
5. Format your code: `dotnet format`
6. Create a pull request

## Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [React TypeScript Cheatsheet](https://react-typescript-cheatsheet.netlify.app/)
