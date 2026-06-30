using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name, string Slug) : IRequest<Guid>;

public class CreateCategoryCommandHandler(ICategoryRepository categoryRepository) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Name)) errors.Add("Name is required.");
        if (string.IsNullOrWhiteSpace(request.Slug)) errors.Add("Slug is required.");
        if (errors.Count > 0) throw new ValidationException(errors);

        var slugNormalized = request.Slug.Trim().ToLowerInvariant();

        if (await categoryRepository.ExistsBySlugAsync(slugNormalized, cancellationToken))
            throw new ConflictException($"A category with slug '{slugNormalized}' already exists.");

        var category = new Category(request.Name.Trim(), slugNormalized);
        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
