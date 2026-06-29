using Microsoft.EntityFrameworkCore;
using RecipeManager.Application.Interfaces;

namespace RecipeManager.Infrastructure.Persistence;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options)
    : DbContext(options), IRecipeDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
