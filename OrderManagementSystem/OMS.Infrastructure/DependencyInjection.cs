using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OMS.Application.Interfaces;
using OMS.Infrastructure.Caching;
using OMS.Infrastructure.Services;

namespace OMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "OMS_";
            });

            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
