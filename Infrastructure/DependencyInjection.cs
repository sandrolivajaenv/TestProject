using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureRegistrations(this IServiceCollection services,
        IConfiguration cfg,
        IWebHostEnvironment env,
        Action<DbContextOptionsBuilder>? configureDb = null)
    {
        // this is used for testing purposes only so we can inject sqllite
        if (env.IsEnvironment("Testing"))
        {
        }
        else if (configureDb == null)
        {
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseSqlServer(cfg.GetConnectionString("LocalConnection")));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(configureDb);
        }


        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
