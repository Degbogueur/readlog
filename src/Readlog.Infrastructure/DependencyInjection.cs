using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Readlog.Infrastructure.Data;
using Readlog.Infrastructure.Identity;
using Readlog.Infrastructure.Interceptors;
using System.IdentityModel.Tokens.Jwt;

namespace Readlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var domainEventInterceptor = sp.GetRequiredService<DomainEventInterceptor>();

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(auditableInterceptor, domainEventInterceptor);
        });
        
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        services.AddSingleton<JwtSecurityTokenHandler>();
        
        return services;
    }
}
