using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Common;
using RecipeManager.Domain.Entities;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Persistence;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IRecipeDbContext
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionRecipe> CollectionRecipes => Set<CollectionRecipe>();
    public DbSet<Rating> Ratings => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeDbContext).Assembly);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.Id))
                    .ValueGeneratedNever();
            }
        }
    }
}
