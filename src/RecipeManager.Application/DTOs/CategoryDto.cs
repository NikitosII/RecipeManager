namespace RecipeManager.Application.DTOs;

public record CategoryDto(Guid Id, string Name, string Slug, int RecipeCount);
