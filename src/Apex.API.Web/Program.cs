using FastEndpoints;
using FastEndpoints.Swagger;
using Apex.API.Infrastructure;
using Apex.API.Web.Configurations;
using Mediator;
using Apex.API.UseCases.Tenants.Create;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
       .AddLoggerConfigs();

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();

startupLogger.LogInformation("Starting web host");

builder.Services.AddOptionConfigs(builder.Configuration, startupLogger, builder);

// Register Mediator
builder.Services.AddMediator(options =>
{
  options.ServiceLifetime = ServiceLifetime.Scoped;
});

// MANUALLY register the handler (until source generator works)
builder.Services.AddScoped<IRequestHandler<CreateTenantCommand, Ardalis.Result.Result<Apex.API.Core.ValueObjects.TenantId>>, CreateTenantHandler>();

// Add Infrastructure (DbContext, Repositories, Tenant Context, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add FastEndpoints
builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                {
                  o.ShortSchemaNames = true;
                });

var app = builder.Build();

// DON'T use HTTPS redirect in development
// app.UseHttpsRedirection();

// FastEndpoints middleware
app.UseFastEndpoints();

// Swagger UI
if (app.Environment.IsDevelopment())
{
  app.UseSwaggerGen();
}

app.MapDefaultEndpoints(); // Aspire health checks

app.Run();

public partial class Program { }