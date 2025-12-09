# LiquorPOS - Microservices Point of Sale System

A comprehensive microservices-based Point of Sale system for liquor stores built with .NET 8, React, and modern cloud-native technologies.

## Architecture

This system follows Clean Architecture principles with a microservices architecture pattern. Each service is independently deployable and maintainable.

### Services

1. **Identity Service** - User authentication and authorization
2. **Catalog Service** - Product catalog management
3. **Inventory & Purchasing Service** - Stock management and supplier orders
4. **Sales & POS Service** - Point of sale transactions
5. **Customer Loyalty Service** - Rewards and customer management
6. **Reporting & Analytics Service** - Business intelligence and insights
7. **Configuration Service** - System-wide settings

### Technology Stack

#### Backend
- **.NET 8** - Runtime and SDK
- **ASP.NET Core** - Web API framework
- **MediatR** - CQRS and mediator pattern
- **FluentValidation** - Input validation
- **Mapster** - Object mapping
- **Entity Framework Core** - ORM
- **Serilog** - Structured logging
- **YARP** - Reverse proxy for API Gateway

#### Frontend
- **React 19** - UI library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool
- **Tailwind CSS** - Utility-first CSS framework
- **Vitest** - Unit testing

#### Infrastructure
- **SQL Server** - Relational database
- **RabbitMQ** - Message broker
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration

## Project Structure

```
LiquorPOS/
├── src/
│   ├── ApiGateway/
│   │   └── LiquorPOS.ApiGateway/          # YARP-based API Gateway
│   ├── BuildingBlocks/
│   │   └── LiquorPOS.BuildingBlocks/      # Shared libraries
│   ├── Frontend/                          # React TypeScript frontend
│   └── Services/
│       ├── Identity/                      # Identity service
│       ├── Catalog/                       # Catalog service
│       ├── InventoryPurchasing/           # Inventory service
│       ├── SalesPOS/                      # Sales service
│       ├── CustomerLoyalty/               # Loyalty service
│       ├── ReportingAnalytics/            # Reporting service
│       └── Configuration/                 # Configuration service
├── tests/
│   └── Services/                          # Unit tests for each service
└── docker-compose.yml                     # Docker orchestration
```

### Service Structure (Clean Architecture)

Each service follows Clean Architecture with four layers:

```
Service/
├── Domain/                    # Domain entities and business logic
├── Application/               # Use cases, DTOs, and interfaces
├── Infrastructure/            # Data access and external services
└── Api/                       # Web API controllers and endpoints
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker and Docker Compose

### Building the Solution

```bash
# Build all .NET projects
dotnet build

# Run tests
dotnet test

# Build frontend
cd src/Frontend
npm install
npm run build
npm run test
```

### Running with Docker Compose

```bash
# Build and start all services
docker compose up --build

# Start in detached mode
docker compose up -d

# Stop all services
docker compose down
```

### Service Endpoints

- **API Gateway**: http://localhost:5000
- **Identity API**: http://localhost:5001
- **Catalog API**: http://localhost:5002
- **Inventory API**: http://localhost:5003
- **Sales API**: http://localhost:5004
- **Loyalty API**: http://localhost:5005
- **Reporting API**: http://localhost:5006
- **Configuration API**: http://localhost:5007
- **Frontend**: http://localhost:3000
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Health Checks

Each service exposes a health check endpoint at `/health`

Example:
```bash
curl http://localhost:5001/health
```

## Development

### Adding a New Feature

1. Create domain entities in the Domain layer
2. Define application use cases using MediatR
3. Implement infrastructure services (repositories, external APIs)
4. Create API endpoints in the API layer
5. Add unit tests in the corresponding test project

### Database Migrations

Each service can maintain its own database:

```bash
cd src/Services/[ServiceName]/LiquorPOS.Services.[ServiceName].Api
dotnet ef migrations add InitialCreate -p ../LiquorPOS.Services.[ServiceName].Infrastructure
dotnet ef database update
```

## Testing

### Backend Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests

```bash
cd src/Frontend
npm run test
```

## Logging

All services use Serilog for structured logging. Logs are written to:
- Console (for Docker)
- Rolling file logs in the `logs/` directory

## BuildingBlocks

The `BuildingBlocks` project contains shared abstractions:

- **Result Pattern** - For handling success/failure responses
- **Domain Exceptions** - Common exception types
- **Auditing** - Base classes for auditable entities
- **Entity Base Classes** - Common entity interfaces and implementations

## Contributing

1. Create a feature branch
2. Make your changes following the existing architecture patterns
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

Proprietary - All rights reserved
