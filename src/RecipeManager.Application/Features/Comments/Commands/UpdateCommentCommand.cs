using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Comments.Commands;

public record UpdateCommentCommand(Guid CommentId, Guid UserId, string Body) : IRequest<CommentDto>;

public class UpdateCommentCommandHandler(ICommentRepository commentRepository) : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var body = CommentValidation.Normalize(request.Body);

        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Comment), request.CommentId);

        comment.EnsureOwnedBy(request.UserId);
        comment.UpdateBody(body);

        commentRepository.Update(comment);
        await commentRepository.SaveChangesAsync(cancellationToken);

        var author = await commentRepository.GetAuthorAsync(comment.UserId, cancellationToken);
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
