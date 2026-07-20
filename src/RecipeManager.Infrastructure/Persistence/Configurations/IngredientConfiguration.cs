using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(i => i.Name).IsUnique();
        builder.Property(i => i.CaloriesPer100g).HasPrecision(8, 2);
        builder.Property(i => i.ProteinPer100g).HasPrecision(8, 2);
        builder.Property(i => i.FatPer100g).HasPrecision(8, 2);
        builder.Property(i => i.CarbsPer100g).HasPrecision(8, 2);
        builder.Property(i => i.FiberPer100g).HasPrecision(8, 2);
        builder.Property(i => i.DensityGramsPerMl).HasPrecision(8, 4);
        builder.Property(i => i.GramsPerPiece).HasPrecision(8, 2);
    }
}
