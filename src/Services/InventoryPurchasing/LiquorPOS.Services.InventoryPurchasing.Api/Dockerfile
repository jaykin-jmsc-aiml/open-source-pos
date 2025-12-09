FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Domain/LiquorPOS.Services.InventoryPurchasing.Domain.csproj", "src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Domain/"]
COPY ["src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Application/LiquorPOS.Services.InventoryPurchasing.Application.csproj", "src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Application/"]
COPY ["src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Infrastructure/LiquorPOS.Services.InventoryPurchasing.Infrastructure.csproj", "src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Infrastructure/"]
COPY ["src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Api/LiquorPOS.Services.InventoryPurchasing.Api.csproj", "src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Api/"]

RUN dotnet restore "src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Api/LiquorPOS.Services.InventoryPurchasing.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/InventoryPurchasing/LiquorPOS.Services.InventoryPurchasing.Api"
RUN dotnet build "LiquorPOS.Services.InventoryPurchasing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.InventoryPurchasing.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.InventoryPurchasing.Api.dll"]
