using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Quantity).HasPrecision(10, 3).IsRequired();
        builder.Property(ri => ri.Unit).IsRequired();

        builder.HasOne(ri => ri.Recipe)
               .WithMany(r => r.RecipeIngredients)
               .HasForeignKey(ri => ri.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Ingredient)
               .WithMany()
               .HasForeignKey(ri => ri.IngredientId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ri => new { ri.RecipeId, ri.IngredientId }).IsUnique();
    }
}
