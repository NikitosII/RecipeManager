using RecipeManager.Domain.Common;

namespace RecipeManager.Domain.Entities;

public class RefreshToken : BaseEntity
{
    protected RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expiryDate)
    {
        UserId = userId;
        Token = token;
        ExpiryDate = expiryDate;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? replacedByToken = null)
    {
        IsRevoked = true;
        ReplacedByToken = replacedByToken;
        DateUpdated = DateTime.UtcNow;
    }
}
