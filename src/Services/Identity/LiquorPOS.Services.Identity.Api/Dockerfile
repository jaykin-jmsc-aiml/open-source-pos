FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/Identity/LiquorPOS.Services.Identity.Domain/LiquorPOS.Services.Identity.Domain.csproj", "src/Services/Identity/LiquorPOS.Services.Identity.Domain/"]
COPY ["src/Services/Identity/LiquorPOS.Services.Identity.Application/LiquorPOS.Services.Identity.Application.csproj", "src/Services/Identity/LiquorPOS.Services.Identity.Application/"]
COPY ["src/Services/Identity/LiquorPOS.Services.Identity.Infrastructure/LiquorPOS.Services.Identity.Infrastructure.csproj", "src/Services/Identity/LiquorPOS.Services.Identity.Infrastructure/"]
COPY ["src/Services/Identity/LiquorPOS.Services.Identity.Api/LiquorPOS.Services.Identity.Api.csproj", "src/Services/Identity/LiquorPOS.Services.Identity.Api/"]

RUN dotnet restore "src/Services/Identity/LiquorPOS.Services.Identity.Api/LiquorPOS.Services.Identity.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Identity/LiquorPOS.Services.Identity.Api"
RUN dotnet build "LiquorPOS.Services.Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.Identity.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.Identity.Api.dll"]
