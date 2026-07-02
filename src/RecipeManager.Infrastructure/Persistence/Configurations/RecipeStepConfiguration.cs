using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeManager.Domain.Entities;

namespace RecipeManager.Infrastructure.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Description).HasMaxLength(1000).IsRequired();

        builder.HasOne(s => s.Recipe)
               .WithMany(r => r.Steps)
               .HasForeignKey(s => s.RecipeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.RecipeId, s.StepNumber }).IsUnique();
    }
}
