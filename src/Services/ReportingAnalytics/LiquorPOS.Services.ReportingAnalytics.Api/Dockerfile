FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BuildingBlocks/LiquorPOS.BuildingBlocks/LiquorPOS.BuildingBlocks.csproj", "src/BuildingBlocks/LiquorPOS.BuildingBlocks/"]
COPY ["src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Domain/LiquorPOS.Services.ReportingAnalytics.Domain.csproj", "src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Domain/"]
COPY ["src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Application/LiquorPOS.Services.ReportingAnalytics.Application.csproj", "src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Application/"]
COPY ["src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Infrastructure/LiquorPOS.Services.ReportingAnalytics.Infrastructure.csproj", "src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Infrastructure/"]
COPY ["src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Api/LiquorPOS.Services.ReportingAnalytics.Api.csproj", "src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Api/"]

RUN dotnet restore "src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Api/LiquorPOS.Services.ReportingAnalytics.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/ReportingAnalytics/LiquorPOS.Services.ReportingAnalytics.Api"
RUN dotnet build "LiquorPOS.Services.ReportingAnalytics.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiquorPOS.Services.ReportingAnalytics.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiquorPOS.Services.ReportingAnalytics.Api.dll"]
