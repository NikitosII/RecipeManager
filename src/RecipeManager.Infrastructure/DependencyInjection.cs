using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Persistence;
using RecipeManager.Infrastructure.Repositories;

namespace RecipeManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RecipeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("RecipeDb")));

        services.AddScoped<IRecipeDbContext>(sp => sp.GetRequiredService<RecipeDbContext>());

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();

        return services;
    }
}
