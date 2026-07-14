using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(f => new { f.UserId, f.RecipeId });

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(f => f.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Recipe)
               .WithMany()
               .HasForeignKey(f => f.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.UserId);
    }
}
