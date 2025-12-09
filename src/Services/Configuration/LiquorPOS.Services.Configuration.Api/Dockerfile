FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/Configuration/LiquorPOS.Services.Configuration.Domain/LiquorPOS.Services.Configuration.Domain.csproj", "src/Services/Configuration/LiquorPOS.Services.Configuration.Domain/"]
COPY ["src/Services/Configuration/LiquorPOS.Services.Configuration.Application/LiquorPOS.Services.Configuration.Application.csproj", "src/Services/Configuration/LiquorPOS.Services.Configuration.Application/"]
COPY ["src/Services/Configuration/LiquorPOS.Services.Configuration.Infrastructure/LiquorPOS.Services.Configuration.Infrastructure.csproj", "src/Services/Configuration/LiquorPOS.Services.Configuration.Infrastructure/"]
COPY ["src/Services/Configuration/LiquorPOS.Services.Configuration.Api/LiquorPOS.Services.Configuration.Api.csproj", "src/Services/Configuration/LiquorPOS.Services.Configuration.Api/"]

RUN dotnet restore "src/Services/Configuration/LiquorPOS.Services.Configuration.Api/LiquorPOS.Services.Configuration.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Configuration/LiquorPOS.Services.Configuration.Api"
RUN dotnet build "LiquorPOS.Services.Configuration.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.Configuration.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.Configuration.Api.dll"]
