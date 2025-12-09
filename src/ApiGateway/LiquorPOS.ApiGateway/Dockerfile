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
