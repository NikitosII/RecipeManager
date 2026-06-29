using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Application.Interfaces;
using RecipeManager.Infrastructure.Persistence;

namespace RecipeManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RecipeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("RecipeDb")));

        services.AddScoped<IRecipeDbContext>(provider =>
            provider.GetRequiredService<RecipeDbContext>());

        return services;
    }
}
