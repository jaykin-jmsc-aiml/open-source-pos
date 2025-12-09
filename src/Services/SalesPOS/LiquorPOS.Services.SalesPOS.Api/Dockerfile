FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Domain/LiquorPOS.Services.SalesPOS.Domain.csproj", "src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Domain/"]
COPY ["src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Application/LiquorPOS.Services.SalesPOS.Application.csproj", "src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Application/"]
COPY ["src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Infrastructure/LiquorPOS.Services.SalesPOS.Infrastructure.csproj", "src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Infrastructure/"]
COPY ["src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Api/LiquorPOS.Services.SalesPOS.Api.csproj", "src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Api/"]

RUN dotnet restore "src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Api/LiquorPOS.Services.SalesPOS.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/SalesPOS/LiquorPOS.Services.SalesPOS.Api"
RUN dotnet build "LiquorPOS.Services.SalesPOS.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.SalesPOS.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.SalesPOS.Api.dll"]
