#!/bin/bash
set -e

# Services to create
SERVICES=("Identity" "Catalog" "InventoryPurchasing" "SalesPOS" "CustomerLoyalty" "ReportingAnalytics" "Configuration")

echo "Creating BuildingBlocks..."
mkdir -p src/BuildingBlocks
dotnet new classlib -n LiquorPOS.BuildingBlocks -o src/BuildingBlocks/LiquorPOS.BuildingBlocks
dotnet sln add src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj

echo "Creating services with Clean Architecture..."
for SERVICE in "${SERVICES[@]}"; do
    echo "Creating $SERVICE service..."
    
    # Create directories
    mkdir -p "src/Services/$SERVICE"
    
    # Domain layer
    dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Domain" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain"
    dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Domain/LiquorPOS.Services.$SERVICE.Domain.csproj"
    
    # Application layer
    dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Application" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application"
    dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Application/LiquorPOS.Services.$SERVICE.Application.csproj"
    
    # Infrastructure layer
    dotnet new classlib -n "LiquorPOS.Services.$SERVICE.Infrastructure" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure"
    dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Infrastructure/LiquorPOS.Services.$SERVICE.Infrastructure.csproj"
    
    # API layer
    dotnet new webapi -n "LiquorPOS.Services.$SERVICE.Api" -o "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api"
    dotnet sln add "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/LiquorPOS.Services.$SERVICE.Api.csproj"
    
    # Unit tests
    dotnet new xunit -n "LiquorPOS.Services.$SERVICE.UnitTests" -o "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests"
    dotnet sln add "tests/Services/$SERVICE/LiquorPOS.Services.$SERVICE.UnitTests/LiquorPOS.Services.$SERVICE.UnitTests.csproj"
done

echo "Creating API Gateway..."
mkdir -p src/ApiGateway
dotnet new webapi -n LiquorPOS.ApiGateway -o src/ApiGateway/LiquorPOS.ApiGateway
dotnet sln add src/ApiGateway/LiquorPOS.ApiGateway/LiquorPOS.ApiGateway.csproj

echo "All projects created successfully!"
