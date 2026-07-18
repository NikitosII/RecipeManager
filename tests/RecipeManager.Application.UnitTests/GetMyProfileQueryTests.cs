using NSubstitute;
using RecipeManager.Application.Features.Users.Queries;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.UnitTests;

public class GetMyProfileQueryTests
{
    private readonly IUserService _users = Substitute.For<IUserService>();
    private readonly IRecipeRepository _recipes = Substitute.For<IRecipeRepository>();

    private GetMyProfileQueryHandler CreateHandler() => new(_users, _recipes);

    [Fact]
    public async Task Handle_CombinesIdentityDetailsWithTheUsersRecipeCount()
    {
        var userId = Guid.NewGuid();
        _users.GetProfileAsync(userId, Arg.Any<CancellationToken>())
            .Returns(("cook@example.com", "Ada", "Lovelace", "/uploads/avatar.png"));
        _recipes.CountByUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(7);

        var result = await CreateHandler().Handle(new GetMyProfileQuery(userId), CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("cook@example.com", result.Email);
        Assert.Equal("Ada", result.FirstName);
        Assert.Equal("Lovelace", result.LastName);
        Assert.Equal("/uploads/avatar.png", result.AvatarUrl);
        Assert.Equal(7, result.RecipeCount);
    }

    [Fact]
    public async Task Handle_WhenTheUserDoesNotExist_Throws()
    {
        var userId = Guid.NewGuid();
        _users.GetProfileAsync(userId, Arg.Any<CancellationToken>())
            .Returns(((string, string, string, string?)?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new GetMyProfileQuery(userId), CancellationToken.None));
    }
}
