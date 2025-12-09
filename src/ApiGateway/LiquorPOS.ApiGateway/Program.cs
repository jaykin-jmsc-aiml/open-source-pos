using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler("/error");
app.UseSerilogRequestLogging();

app.MapReverseProxy();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "API Gateway", status = "running" }));

app.Run();
