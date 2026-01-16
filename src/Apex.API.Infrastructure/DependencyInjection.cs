using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using Traxs.SharedKernel;
using Apex.API.Core.Interfaces;
using Apex.API.Core.Aggregates.UserAggregate;
using Apex.API.Infrastructure.Data;
using Apex.API.Infrastructure.Identity;
using Apex.API.Infrastructure.Services;
using Apex.API.UseCases.Common.Interfaces;
using Apex.API.UseCases.Common.Behaviors;
using Hangfire;
using Hangfire.SqlServer;
using SendGrid.Extensions.DependencyInjection;
using Apex.API.Infrastructure.Email;

namespace Apex.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // DbContext with Identity
        services.AddDbContext<ApexDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApexDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ========================================================================
        // IDENTITY: User management with ASP.NET Core Identity
        // ========================================================================
        services.AddIdentity<User, Role>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign in settings
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Token settings
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
        })
        .AddEntityFrameworkStores<ApexDbContext>()
        .AddDefaultTokenProviders();

        // Configure token lifespan (default is 1 day)
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        // ========================================================================
        // JWT AUTHENTICATION
        // ========================================================================
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // HTTP Context
        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        // Tenant context
        services.AddScoped<ITenantContext, TenantContext>();

        // Repository pattern
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, Apex.API.Infrastructure.Services.MediatorDomainEventDispatcher>();

        // Services
        services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>(); // ✅ NEW

        // ========================================================================
        // MEDIATR: Auto-discovers all handlers! ✨
        // ========================================================================
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Apex.API.UseCases.Tenants.Create.CreateTenantHandler).Assembly);

            // Add validation pipeline behavior
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // ========================================================================
        // FLUENTVALIDATION: Auto-discovers all validators! ✨
        // ========================================================================
        services.AddValidatorsFromAssembly(
            typeof(Apex.API.UseCases.Tenants.Create.CreateTenantCommand).Assembly);

        // ========================================================================
        // SENDGRID: Email service configuration
        // ========================================================================
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        var emailProvider = configuration.GetValue<string>("Email:Provider") ?? "Console";

        if (emailProvider == "SendGrid")
        {
            var apiKey = configuration.GetValue<string>("Email:SendGridApiKey");
            services.AddSendGrid(options => { options.ApiKey = apiKey; });
            services.AddScoped<IEmailService, SendGridEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, ConsoleEmailService>();
        }

        // ========================================================================
        // HANGFIRE: Background job processing
        // ========================================================================
        var hangfireEnabled = configuration.GetValue<bool>("Hangfire:Enabled");
        if (hangfireEnabled)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = configuration.GetValue<int>("Hangfire:WorkerCount", 5);
            });
        }




        return services;
    }
}
