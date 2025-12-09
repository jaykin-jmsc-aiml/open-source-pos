FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/Catalog/LiquorPOS.Services.Catalog.Domain/LiquorPOS.Services.Catalog.Domain.csproj", "src/Services/Catalog/LiquorPOS.Services.Catalog.Domain/"]
COPY ["src/Services/Catalog/LiquorPOS.Services.Catalog.Application/LiquorPOS.Services.Catalog.Application.csproj", "src/Services/Catalog/LiquorPOS.Services.Catalog.Application/"]
COPY ["src/Services/Catalog/LiquorPOS.Services.Catalog.Infrastructure/LiquorPOS.Services.Catalog.Infrastructure.csproj", "src/Services/Catalog/LiquorPOS.Services.Catalog.Infrastructure/"]
COPY ["src/Services/Catalog/LiquorPOS.Services.Catalog.Api/LiquorPOS.Services.Catalog.Api.csproj", "src/Services/Catalog/LiquorPOS.Services.Catalog.Api/"]

RUN dotnet restore "src/Services/Catalog/LiquorPOS.Services.Catalog.Api/LiquorPOS.Services.Catalog.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Catalog/LiquorPOS.Services.Catalog.Api"
RUN dotnet build "LiquorPOS.Services.Catalog.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.Catalog.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.Catalog.Api.dll"]
