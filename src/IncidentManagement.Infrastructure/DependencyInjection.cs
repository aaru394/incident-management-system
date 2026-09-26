using IncidentManagement.Application.Interfaces;
using IncidentManagement.Application.Services;
using IncidentManagement.Infrastructure.Auth;
using IncidentManagement.Infrastructure.Caching;
using IncidentManagement.Infrastructure.Options;
using IncidentManagement.Infrastructure.Persistence;
using IncidentManagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace IncidentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
