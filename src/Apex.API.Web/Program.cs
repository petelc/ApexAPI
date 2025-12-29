using FastEndpoints;
using FastEndpoints.Swagger;
using Apex.API.Infrastructure;
using Apex.API.Web.Configurations;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddLoggerConfigs();

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();

startupLogger.LogInformation("Starting web host");

builder.Services.AddOptionConfigs(builder.Configuration, startupLogger, builder);

// Register Mediator (REQUIRED - used by Infrastructure's MediatorDomainEventDispatcher)
builder.Services.AddMediator(options =>
{
  options.ServiceLifetime = ServiceLifetime.Scoped;
});

// Add Infrastructure (DbContext, Repositories, Tenant Context, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints with Swagger configuration
builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
      o.ShortSchemaNames = true;

      // Configure document settings
      o.DocumentSettings = settings =>
      {
        settings.Title = "APEX Multi-Tenant API";
        settings.Version = "v1";
        settings.Description = "A production-ready multi-tenant SaaS platform built with Clean Architecture and DDD";

        // PostProcess is called when the document is generated
        settings.PostProcess = document =>
          {
            // Add server URLs for tenant switching in Swagger UI
            document.Servers.Clear(); // Clear any default servers

            document.Servers.Add(new NSwag.OpenApiServer
            {
              Url = "https://demo.localhost:5000",
              Description = "Demo Tenant (Professional Tier)"
            });

            document.Servers.Add(new NSwag.OpenApiServer
            {
              Url = "https://test.localhost:5000",
              Description = "Test Tenant (Starter Tier)"
            });

            document.Servers.Add(new NSwag.OpenApiServer
            {
              Url = "https://acmecorp.localhost:5000",
              Description = "Acme Corporation (Trial)"
            });

            document.Servers.Add(new NSwag.OpenApiServer
            {
              Url = "https://localhost:5000",
              Description = "No Tenant (Will fail for /current endpoint)"
            });
          };
      };
    });

var app = builder.Build();

// DON'T use HTTPS redirect in development (commented out)
// app.UseHttpsRedirection();

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
    // Customize Swagger UI
    c.ConfigureDefaults();

    // Optional: Persist authorization data
    c.PersistAuthorization = true;
  });
}

app.MapDefaultEndpoints(); // Aspire health checks

app.Run();

public partial class Program { }