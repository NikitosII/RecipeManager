namespace RecipeManager.Application.Interfaces;

public interface IRecipeDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
