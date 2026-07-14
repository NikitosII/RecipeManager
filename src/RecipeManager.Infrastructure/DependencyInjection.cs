using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Application.Configuration;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Interfaces;
using RecipeManager.Infrastructure.Identity;
using RecipeManager.Infrastructure.Persistence;
using RecipeManager.Infrastructure.Repositories;
using RecipeManager.Infrastructure.Services;

namespace RecipeManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<RecipeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("RecipeDb")));

        services.AddScoped<IRecipeDbContext>(sp => sp.GetRequiredService<RecipeDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;

            // Brute-force protection: lock the account for 15 minutes after 5 consecutive failed sign-in attempts.
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<RecipeDbContext>();

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
