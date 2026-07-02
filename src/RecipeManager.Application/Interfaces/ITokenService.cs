using RecipeManager.Application.DTOs;

namespace RecipeManager.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime Expiry) GenerateAccessToken(UserClaimsDto user);
    (string Token, DateTime Expiry) GenerateRefreshToken();
}
