using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Logging;
using TripEnjoy.Application.Interfaces.Payment;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Application.Messages.Consumers;
using TripEnjoy.Infrastructure.MessageBroker;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Infrastructure.Persistence.Repositories;
using TripEnjoy.Infrastructure.Services;
using TripEnjoy.Infrastructure.Services.CloudStorage;
using TripEnjoy.Infrastructure.Services.Payment;
using TripEnjoy.Infrastructure.Logging;
using TripEnjoy.ShareKernel.Email;

namespace TripEnjoy.Infrastructure
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailConfiguration>(configuration.GetSection("EMAIL_CONFIGURATION"));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
                options.InstanceName = "TripEnjoy_";
            });

            services.AddDbContext<TripEnjoyDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(TripEnjoyDbContext).Assembly.FullName)));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.SignIn.RequireConfirmedEmail = true;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;
                }
            )
                .AddEntityFrameworkStores<TripEnjoyDbContext>()
                .AddDefaultTokenProviders();


            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IPartnerDocumentRepository, PartnerDocumentRepository>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            
            // Cloud Storage Services
            services.AddHttpClient<CloudinaryService>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();

            // Enhanced Logging Service
            services.AddSingleton<ILogService, LogService>();

            // Payment Services
            services.Configure<VNPayConfiguration>(configuration.GetSection("VNPay"));
            services.AddScoped<IPaymentService, VNPayPaymentService>();

            // Message Broker (RabbitMQ with MassTransit) - skip in Testing environment
            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "";
            if (environment != "Testing")
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
                
                services.AddMassTransit(x =>
                {
                    // Add all consumers from the Application layer
                    x.AddConsumer<BookingCreatedConsumer>();
                    x.AddConsumer<BookingConfirmedConsumer>();
                    x.AddConsumer<BookingCancelledConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(rabbitMqSettings.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, h =>
                        {
                            h.Username(rabbitMqSettings.Username);
                            h.Password(rabbitMqSettings.Password);
                        });

                        // Configure retry policy
                        cfg.UseMessageRetry(r => r.Intervals(
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(30)
                        ));

                        // Configure endpoints for consumers
                        cfg.ConfigureEndpoints(context);
                    });
                });
            }

            return services;
        }
    }
}
