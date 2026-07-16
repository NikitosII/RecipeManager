using MediatR;
using RecipeManager.Application.DTOs;
using RecipeManager.Application.Interfaces;
using RecipeManager.Domain.Exceptions;
using RecipeManager.Domain.Interfaces;

namespace RecipeManager.Application.Features.Users.Queries;

public record GetMyProfileQuery(Guid UserId) : IRequest<UserProfileDto>;

public class GetMyProfileQueryHandler(
    IUserService userService,
    IRecipeRepository recipeRepository) : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await userService.GetProfileAsync(request.UserId, cancellationToken)
                      ?? throw new NotFoundException("User", request.UserId);

        var recipeCount = await recipeRepository.CountByUserAsync(request.UserId, cancellationToken);

        return new UserProfileDto(
            request.UserId,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.AvatarUrl,
            recipeCount);
    }
}
