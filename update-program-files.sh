#!/bin/bash
set -e

SERVICES=("Identity" "Catalog" "InventoryPurchasing" "SalesPOS" "CustomerLoyalty" "ReportingAnalytics" "Configuration")

for SERVICE in "${SERVICES[@]}"; do
    echo "Updating Program.cs for $SERVICE service..."
    
    cat > "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/Program.cs" << 'PROGRAMCS'
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler("/error");
app.UseSerilogRequestLogging();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "SERVICE_NAME", status = "running" }))
    .WithName("Root")
    .WithOpenApi();

app.Run();
PROGRAMCS

    sed -i "s/SERVICE_NAME/$SERVICE/" "src/Services/$SERVICE/LiquorPOS.Services.$SERVICE.Api/Program.cs"
done

echo "All Program.cs files updated!"
