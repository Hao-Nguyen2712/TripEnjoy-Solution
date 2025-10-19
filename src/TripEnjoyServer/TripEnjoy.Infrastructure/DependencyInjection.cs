using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.Application.Common.Interfaces;
using TripEnjoy.Application.Interfaces.External.Cache;
using TripEnjoy.Application.Interfaces.External.CloudStorage;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Infrastructure.Persistence.Repositories;
using TripEnjoy.Infrastructure.Services;
using TripEnjoy.Infrastructure.Services.CloudStorage;
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
                options.UseSqlServer(
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


            return services;
        }
    }
}
