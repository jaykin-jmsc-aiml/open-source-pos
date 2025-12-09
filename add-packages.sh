#!/bin/bash
set -e

SERVICES=("Identity" "Catalog" "InventoryPurchasing" "SalesPOS" "CustomerLoyalty" "ReportingAnalytics" "Configuration")

echo "Adding packages to services..."
for SERVICE in "${SERVICES[@]}"; do
    echo "Configuring $SERVICE service..."
    
    # Add packages to Application layer
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" package MediatR
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" package FluentValidation
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" package FluentValidation.DependencyInjectionExtensions
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" package Mapster
    
    # Add packages to Infrastructure layer
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design --version 8.0.11
    
    # Add packages to API layer
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" package Serilog.AspNetCore
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" package Serilog.Sinks.Console
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" package Serilog.Sinks.File
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" package Microsoft.AspNetCore.Diagnostics.HealthChecks
    
    # Add project references
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain/LiquorPOS.Services.$SERVICE.Domain.csproj"
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj" reference "src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj"
    
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj"
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj" reference "src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj"
    
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj"
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj"
    dotnet add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj" reference "src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj"
    
    # Add test references
    dotnet add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/LiquorPOS.Services.$SERVICE.UnitTests.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain/LiquorPOS.Services.$SERVICE.Domain.csproj"
    dotnet add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/LiquorPOS.Services.$SERVICE.UnitTests.csproj" reference "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj"
    dotnet add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/LiquorPOS.Services.$SERVICE.UnitTests.csproj" package FluentAssertions
    dotnet add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/LiquorPOS.Services.$SERVICE.UnitTests.csproj" package Moq
done

echo "Adding packages to API Gateway..."
dotnet add src/ApiGateway/LiquorPOS.ApiGateway/LiquorPOS.ApiGateway.csproj package Yarp.ReverseProxy
dotnet add src/ApiGateway/LiquorPOS.ApiGateway/LiquorPOS.ApiGateway.csproj package Serilog.AspNetCore

echo "All packages added successfully!"
