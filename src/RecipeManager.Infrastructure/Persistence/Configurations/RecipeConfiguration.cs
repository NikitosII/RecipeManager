using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.ImageUrl).HasMaxLength(500);
        builder.Property(r => r.DifficultyLevel).IsRequired();

        builder.HasOne(r => r.User)
               .WithMany(u => u.Recipes)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Steps)
               .HasField("_steps")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(r => r.RecipeIngredients)
               .HasField("_recipeIngredients")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
