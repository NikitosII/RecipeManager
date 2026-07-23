using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Comments.Queries;

public record GetCommentsForRecipeQuery(Guid RecipeId, Guid? RequestingUserId = null) : IRequest<IReadOnlyList<CommentDto>>;

public class GetCommentsForRecipeQueryHandler(ICommentRepository commentRepository) : IRequestHandler<GetCommentsForRecipeQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(GetCommentsForRecipeQuery request, CancellationToken cancellationToken)
    {
        var comments = await commentRepository.GetForRecipeAsync(request.RecipeId, cancellationToken);

        return comments.Select(c => new CommentDto(
            c.Id,
            c.UserId,
            c.AuthorName,
            c.AuthorAvatarUrl,
            c.Body,
            CanEdit: request.RequestingUserId is { } userId && userId == c.UserId,
            c.DateCreated,
            c.DateUpdated)).ToList();
    }
}
