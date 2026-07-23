using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;
using RecipeManager.Infrastructure.Identity;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Body)
               .IsRequired()
               .HasMaxLength(Comment.MaxLength);

        builder.HasOne<ApplicationUser>()
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Recipe)
               .WithMany()
               .HasForeignKey(c => c.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        // Comments are listed per recipe, ordered by creation time.
        builder.HasIndex(c => new { c.RecipeId, c.DateCreated });
    }
}
