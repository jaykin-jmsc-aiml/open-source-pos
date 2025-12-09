FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Domain/LiquorPOS.Services.CustomerLoyalty.Domain.csproj", "src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Domain/"]
COPY ["src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Application/LiquorPOS.Services.CustomerLoyalty.Application.csproj", "src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Application/"]
COPY ["src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Infrastructure/LiquorPOS.Services.CustomerLoyalty.Infrastructure.csproj", "src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Infrastructure/"]
COPY ["src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Api/LiquorPOS.Services.CustomerLoyalty.Api.csproj", "src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Api/"]

RUN dotnet restore "src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Api/LiquorPOS.Services.CustomerLoyalty.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/CustomerLoyalty/LiquorPOS.Services.CustomerLoyalty.Api"
RUN dotnet build "LiquorPOS.Services.CustomerLoyalty.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.CustomerLoyalty.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.CustomerLoyalty.Api.dll"]
