using FastEndpoints;
using FastEndpoints.Swagger;
using Apex.API.Infrastructure;
using Apex.API.Infrastructure.Data;
using Apex.API.Web.Configurations;
using Hangfire;
using Apex.API.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddLoggerConfigs();

// ✅ CORS Configuration for React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")  // React dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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

// ✅ IMPORTANT: Use CORS before Authentication/Authorization
app.UseCors("ReactDevPolicy");

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

// Hangfire Dashboard
var hangfireEnabled = app.Configuration.GetValue<bool>("Hangfire:Enabled");
if (hangfireEnabled)
{
    var dashboardPath = app.Configuration.GetValue<string>("Hangfire:DashboardPath") ?? "/hangfire";

    app.UseHangfireDashboard(dashboardPath, new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

app.MapDefaultEndpoints();

app.Run();

public partial class Program { }
