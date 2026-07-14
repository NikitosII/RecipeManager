using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class CollectionRecipeConfiguration : IEntityTypeConfiguration<CollectionRecipe>
{
    public void Configure(EntityTypeBuilder<CollectionRecipe> builder)
    {
        builder.HasKey(cr => new { cr.CollectionId, cr.RecipeId });

        builder.HasOne(cr => cr.Collection)
               .WithMany(c => c.Recipes)
               .HasForeignKey(cr => cr.CollectionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.Recipe)
               .WithMany()
               .HasForeignKey(cr => cr.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
