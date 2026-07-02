namespace RecipeManager.Application.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenExpiryMinutes { get; init; } = 30;
    public int RefreshTokenExpiryDays { get; init; } = 7;
    public string SigningKey { get; init; } = default!;
}
