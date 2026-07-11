using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence;

public static class DbSeeder
{
    private static readonly (string Name, string Slug)[] DefaultCategories =
    [
        ("Breakfast", "breakfast"),
        ("Lunch", "lunch"),
        ("Dinner", "dinner"),
        ("Vegan", "vegan"),
        ("Dessert", "dessert"),
        ("Quick", "quick"),
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            foreach (var (name, slug) in DefaultCategories)
                await context.Categories.AddAsync(new Category(name, slug), cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
