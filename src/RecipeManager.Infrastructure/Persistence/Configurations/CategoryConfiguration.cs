using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasMany(c => c.Recipes)
               .WithOne(r => r.Category)
               .HasForeignKey(r => r.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
