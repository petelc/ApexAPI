using FastEndpoints;
using FastEndpoints.Swagger;
using Apex.API.Infrastructure;
using Apex.API.Infrastructure.Data;
using Apex.API.Web.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddLoggerConfigs();

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();

startupLogger.LogInformation("Starting web host");

builder.Services.AddOptionConfigs(builder.Configuration, startupLogger, builder);

// Add Infrastructure (DbContext, Repositories, MediatR, Identity, JWT, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Authorization services
builder.Services.AddAuthorization();

// Add FastEndpoints with Swagger configuration
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
        
        o.DocumentSettings = settings =>
        {
            settings.Title = "APEX Multi-Tenant API";
            settings.Version = "v1";
            settings.Description = "A production-ready multi-tenant SaaS platform with authentication";
            
            settings.PostProcess = document =>
            {
                document.Servers.Clear();
                
                document.Servers.Add(new NSwag.OpenApiServer
                {
                    Url = "https://demo.localhost:5000",
                    Description = "Demo Tenant"
                });
                
                document.Servers.Add(new NSwag.OpenApiServer
                {
                    Url = "https://localhost:5000",
                    Description = "No Tenant"
                });
            };
        };
    });

var app = builder.Build();

// ✅ SEED DATABASE (roles, etc.)
await DatabaseSeeder.SeedAsync(app.Services);

// Authentication & Authorization BEFORE endpoints
app.UseAuthentication();
app.UseAuthorization();

// FastEndpoints middleware
app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
});

// Swagger UI (only in development)
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(uiConfig: c =>
    {
        c.ConfigureDefaults();
        c.PersistAuthorization = true;
    });
}

app.MapDefaultEndpoints();

app.Run();

public partial class Program { }
