using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripEnjoy.Application.Interfaces.External.Email;
using TripEnjoy.Application.Interfaces.Identity;
using TripEnjoy.Application.Interfaces.Persistence;
using TripEnjoy.Infrastructure.Persistence;
using TripEnjoy.Infrastructure.Persistence.Repositories;
using TripEnjoy.Infrastructure.Services;
using TripEnjoy.ShareKernel.Email;

namespace TripEnjoy.Infrastructure
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Configures infrastructure services (persistence, caching, identity, email, and domain services) and returns the updated service collection.
        /// </summary>
        /// <remarks>
        /// - Binds EmailConfiguration from the "EMAIL_CONFIGURATION" configuration section.
        /// - Configures StackExchange.Redis distributed cache using "CacheSettings:ConnectionString" and instance name "TripEnjoy_".
        /// - Registers TripEnjoyDbContext with SQL Server using the "DefaultConnection" connection string and sets the migrations assembly to the DbContext's assembly.
        /// - Configures ASP.NET Core Identity for ApplicationUser and IdentityRole with required confirmed account/email and lockout settings (5 minute lockout, 5 max failed attempts, allowed for new users). Adds EF stores and default token providers.
        /// - Registers scoped implementations: IAuthenService, IUnitOfWork, IGenericRepository<>, and IEmailService.
        /// </remarks>
        /// <returns>The same IServiceCollection instance with infrastructure services registered.</returns>
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

            return services;
        }
    }
}
