using NSubstitute;
using RecipeManager.Application.Features.Comments.Commands;
using RecipeManager.Application.Features.Comments.Queries;
using RecipeManager.Domain.Entities;
using RecipeManager.Domain.Enums;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class CommentCommandTests
{
    private readonly ICommentRepository _comments = Substitute.For<ICommentRepository>();
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private static Recipe NewRecipe() =>
        new("Soup", null, DifficultyLevel.Easy, 5, 20, 4, Guid.NewGuid(), Guid.NewGuid());

    // -- Add -- //

    [Fact]
    public async Task Add_ValidComment_PersistsAndReturnsOwnedDto()
    {
        var recipe = NewRecipe();
        var userId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipe.Id, Arg.Any<CancellationToken>()).Returns(recipe);
        _comments.GetAuthorAsync(userId, Arg.Any<CancellationToken>()).Returns(("Emma Stone", "/uploads/emma.jpg"));

        Comment? added = null;
        await _comments.AddAsync(Arg.Do<Comment>(c => added = c), Arg.Any<CancellationToken>());

        var handler = new AddCommentCommandHandler(_comments, _recipes);
        var dto = await handler.Handle(new AddCommentCommand(recipe.Id, userId, "  Looks delicious!  "), CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal("Looks delicious!", added!.Body); // trimmed
        Assert.Equal("Looks delicious!", dto.Body);
        Assert.Equal("Emma Stone", dto.AuthorName);
        Assert.Equal("/uploads/emma.jpg", dto.AuthorAvatarUrl);
        Assert.True(dto.CanEdit);
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Add_MissingRecipe_ThrowsNotFound()
    {
        var recipeId = Guid.NewGuid();
        _recipes.GetByIdAsync(recipeId, Arg.Any<CancellationToken>()).Returns((Recipe?)null);

        var handler = new AddCommentCommandHandler(_comments, _recipes);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new AddCommentCommand(recipeId, Guid.NewGuid(), "Hi"), CancellationToken.None));
        await _comments.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Add_BlankBody_ThrowsValidation(string body)
    {
        var handler = new AddCommentCommandHandler(_comments, _recipes);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), body), CancellationToken.None));
    }

    [Fact]
    public async Task Add_OverLongBody_ThrowsValidation()
    {
        var handler = new AddCommentCommandHandler(_comments, _recipes);
        var tooLong = new string('x', Comment.MaxLength + 1);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), tooLong), CancellationToken.None));
    }

    // -- Update -- //

    [Fact]
    public async Task Update_ByAuthor_ChangesBody()
    {
        var userId = Guid.NewGuid();
        var comment = new Comment(Guid.NewGuid(), userId, "old");
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);
        _comments.GetAuthorAsync(userId, Arg.Any<CancellationToken>()).Returns(("Marco Polo", (string?)null));

        var handler = new UpdateCommentCommandHandler(_comments);
        var dto = await handler.Handle(new UpdateCommentCommand(comment.Id, userId, "new body"), CancellationToken.None);

        Assert.Equal("new body", comment.Body);
        Assert.Equal("new body", dto.Body);
        _comments.Received(1).Update(comment);
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_ByNonAuthor_ThrowsForbidden()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "old");
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var handler = new UpdateCommentCommandHandler(_comments);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new UpdateCommentCommand(comment.Id, Guid.NewGuid(), "hijack"), CancellationToken.None));
        await _comments.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_MissingComment_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        _comments.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Comment?)null);

        var handler = new UpdateCommentCommandHandler(_comments);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new UpdateCommentCommand(id, Guid.NewGuid(), "body"), CancellationToken.None));
    }

    // -- Delete -- //

    [Fact]
    public async Task Delete_ByAuthor_Removes()
    {
        var userId = Guid.NewGuid();
        var comment = new Comment(Guid.NewGuid(), userId, "bye");
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var handler = new DeleteCommentCommandHandler(_comments);
        await handler.Handle(new DeleteCommentCommand(comment.Id, userId), CancellationToken.None);

        _comments.Received(1).Delete(comment);
        await _comments.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ByNonAuthor_ThrowsForbidden()
    {
        var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "bye");
        _comments.GetByIdAsync(comment.Id, Arg.Any<CancellationToken>()).Returns(comment);

        var handler = new DeleteCommentCommandHandler(_comments);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteCommentCommand(comment.Id, Guid.NewGuid()), CancellationToken.None));
        _comments.DidNotReceive().Delete(Arg.Any<Comment>());
    }

    // -- Query -- //

    [Fact]
    public async Task GetForRecipe_MarksOnlyRequestersOwnCommentsEditable()
    {
        var recipeId = Guid.NewGuid();
        var me = Guid.NewGuid();
        var other = Guid.NewGuid();
        _comments.GetForRecipeAsync(recipeId, Arg.Any<CancellationToken>()).Returns(new List<CommentWithAuthor>
        {
            new(Guid.NewGuid(), me, "Me", null, "mine", DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), other, "Other", null, "theirs", DateTime.UtcNow, DateTime.UtcNow),
        });

        var handler = new GetCommentsForRecipeQueryHandler(_comments);
        var result = await handler.Handle(new GetCommentsForRecipeQuery(recipeId, me), CancellationToken.None);

        Assert.True(result.Single(c => c.UserId == me).CanEdit);
        Assert.False(result.Single(c => c.UserId == other).CanEdit);
    }

    [Fact]
    public async Task GetForRecipe_AnonymousRequester_NothingEditable()
    {
        var recipeId = Guid.NewGuid();
        _comments.GetForRecipeAsync(recipeId, Arg.Any<CancellationToken>()).Returns(new List<CommentWithAuthor>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Someone", null, "hi", DateTime.UtcNow, DateTime.UtcNow),
        });

        var handler = new GetCommentsForRecipeQueryHandler(_comments);
        var result = await handler.Handle(new GetCommentsForRecipeQuery(recipeId, null), CancellationToken.None);

        Assert.All(result, c => Assert.False(c.CanEdit));
    }
}
