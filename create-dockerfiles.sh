#!/bin/bash
set -e

SERVICES=("Identity" "Catalog" "InventoryPurchasing" "SalesPOS" "CustomerLoyalty" "ReportingAnalytics" "Configuration")

for SERVICE in "${SERVICES[@]}"; do
    echo "Creating Dockerfile for $SERVICE service..."
    
    cat > "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/Dockerfile" << 'DOCKERFILE'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Domain/LiquorPOS.Services.SERVICE_NAME.Domain.csproj", "src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Domain/"]
COPY ["src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Application/LiquorPOS.Services.SERVICE_NAME.Application.csproj", "src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Application/"]
COPY ["src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Infrastructure/LiquorPOS.Services.SERVICE_NAME.Infrastructure.csproj", "src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Infrastructure/"]
COPY ["src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Api/LiquorPOS.Services.SERVICE_NAME.Api.csproj", "src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Api/"]

RUN dotnet restore "src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Api/LiquorPOS.Services.SERVICE_NAME.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/SERVICE_NAME/LiquorPOS.Services.SERVICE_NAME.Api"
RUN dotnet build "LiquorPOS.Services.SERVICE_NAME.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.SERVICE_NAME.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.SERVICE_NAME.Api.dll"]
DOCKERFILE

    sed -i "s/SERVICE_NAME/$SERVICE/g" "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/Dockerfile"
done

echo "Creating Dockerfile for API Gateway..."
cat > "src/ApiGateway/LiquorPOS.ApiGateway/Dockerfile" << 'DOCKERFILE'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/ApiGateway/LiquorPOS.ApiGateway/LiquorPOS.ApiGateway.csproj", "src/ApiGateway/LiquorPOS.ApiGateway/"]

RUN dotnet restore "src/ApiGateway/LiquorPOS.ApiGateway/LiquorPOS.ApiGateway.csproj"

COPY . .
WORKDIR "/src/src/ApiGateway/LiquorPOS.ApiGateway"
RUN dotnet build "LiquorPOS.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.ApiGateway.dll"]
DOCKERFILE

echo "All Dockerfiles created!"
