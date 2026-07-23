using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Comments.Commands;

public record AddCommentCommand(Guid RecipeId, Guid UserId, string Body) : IRequest<CommentDto>;

public class AddCommentCommandHandler(ICommentRepository commentRepository, IRecipeRepository recipeRepository) : IRequestHandler<AddCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var body = CommentValidation.Normalize(request.Body);

        _ = await recipeRepository.GetByIdAsync(request.RecipeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipe), request.RecipeId);

        var comment = new Comment(request.RecipeId, request.UserId, body);
        await commentRepository.AddAsync(comment, cancellationToken);
        await commentRepository.SaveChangesAsync(cancellationToken);

        var author = await commentRepository.GetAuthorAsync(request.UserId, cancellationToken);
        return new CommentDto(
            comment.Id,
            comment.UserId,
            author?.AuthorName ?? "Unknown",
            author?.AuthorAvatarUrl,
            comment.Body,
            CanEdit: true,
            comment.DateCreated,
            comment.DateUpdated);
    }
}
