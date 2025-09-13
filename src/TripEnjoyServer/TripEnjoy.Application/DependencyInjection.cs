using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TripEnjoy.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers application-layer services into the provided <see cref="IServiceCollection"/>.
        /// Currently this adds MediatR registrations from the executing assembly and returns the collection for chaining.
        /// </summary>
        /// <param name="configuration">Configuration instance (not used by this method currently; reserved for future registrations).</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance after registrations.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
