using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Recipes)
               .HasField("_recipes")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.UserId);
    }
}
