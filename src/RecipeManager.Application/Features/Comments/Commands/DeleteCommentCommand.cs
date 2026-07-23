using MediatR;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Comments.Commands;

public record DeleteCommentCommand(Guid CommentId, Guid UserId) : IRequest;

public class DeleteCommentCommandHandler(ICommentRepository commentRepository) : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.GetByIdAsync(request.CommentId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Comment), request.CommentId);

        comment.EnsureOwnedBy(request.UserId);

        commentRepository.Delete(comment);
        await commentRepository.SaveChangesAsync(cancellationToken);
    }
}
