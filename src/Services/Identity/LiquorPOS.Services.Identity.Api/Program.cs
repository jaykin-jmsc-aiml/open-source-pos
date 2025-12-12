using System;
using System.Text;
using FluentValidation;
using LiquorPOS.Services.Identity.Application.Commands.AssignUserRoles;
using LiquorPOS.Services.Identity.Application.Commands.Login;
using LiquorPOS.Services.Identity.Application.Commands.RefreshToken;
using LiquorPOS.Services.Identity.Application.Commands.Register;
using LiquorPOS.Services.Identity.Application.Commands.RevokeToken;
using LiquorPOS.Services.Identity.Domain.Options;
using LiquorPOS.Services.Identity.Application.Queries;
using LiquorPOS.Services.Identity.Domain.Services;
using LiquorPOS.Services.Identity.Domain.Entities;
using LiquorPOS.Services.Identity.Infrastructure.Persistence;
using LiquorPOS.Services.Identity.Infrastructure.Seeding;
using LiquorPOS.Services.Identity.Infrastructure.Security;
using LiquorPOS.Services.Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

var useInMemory = Environment.GetEnvironmentVariable("USE_IN_MEMORY_DB") == "true";

if (useInMemory)
{
    builder.Services.AddDbContext<LiquorPOSIdentityDbContext>(options =>
        options.UseInMemoryDatabase("LiquorPOSIdentityDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("IdentityDatabase")
        ?? throw new InvalidOperationException("Connection string 'IdentityDatabase' was not found.");

    builder.Services.AddDbContext<LiquorPOSIdentityDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(LiquorPOSIdentityDbContext).Assembly.FullName);
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        }));
}

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

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing");

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITokenValidator, TokenValidator>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection("Identity:Seed"));
builder.Services.AddScoped<IIdentitySeeder, IdentitySeeder>();
builder.Services.AddHostedService<IdentitySeederHostedService>();

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblies(
        typeof(RegisterCommand).Assembly,
        typeof(LoginCommand).Assembly,
        typeof(RefreshTokenCommand).Assembly,
        typeof(RevokeTokenCommand).Assembly,
        typeof(AssignUserRolesCommand).Assembly,
        typeof(GetUsersQuery).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommand>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new { service = "Identity", status = "running" }))
    .WithName("Root")
    .WithOpenApi();

app.Run();

public partial class Program { }
