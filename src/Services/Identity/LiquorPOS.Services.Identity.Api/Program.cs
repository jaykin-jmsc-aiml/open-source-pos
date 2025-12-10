using System;
using LiquorPOS.Services.Identity.Infrastructure.Identity;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Seeding;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

var connectionString = builder.Configuration.GetConnectionString("IdentityDatabase")
    ?? throw new InvalidOperationException("Connection string 'IdentityDatabase' was not found.");

builder.Services.AddDbContext<LiquorPOSIdentityDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(LiquorPOSIdentityDbContext).Assembly.FullName);
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

builder.Services.AddDataProtection();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<LiquorPOSIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, DomainPasswordHasher>();

builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection("Identity:Seed"));
builder.Services.AddScoped<IIdentitySeeder, IdentitySeeder>();
builder.Services.AddHostedService<IdentitySeederHostedService>();

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

app.MapGet("/", () => Results.Ok(new { service = "Identity", status = "running" }))
    .WithName("Root")
    .WithOpenApi();

app.Run();
