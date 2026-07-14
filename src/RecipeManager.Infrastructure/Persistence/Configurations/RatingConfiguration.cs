using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => new { r.UserId, r.RecipeId });

        builder.Property(r => r.Value).IsRequired();

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Recipe)
               .WithMany()
               .HasForeignKey(r => r.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.RecipeId);
    }
}
