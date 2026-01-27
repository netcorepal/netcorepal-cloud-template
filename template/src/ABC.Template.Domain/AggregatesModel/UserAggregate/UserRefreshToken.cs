using ABC.Template.Domain;

namespace ABC.Template.Domain.AggregatesModel.UserAggregate;

public partial record UserRefreshTokenId : IInt64StronglyTypedId;

public class UserRefreshToken : Entity<UserRefreshTokenId>
{
    protected UserRefreshToken()
    {
    }

    public UserRefreshToken(string token)
    {
        Token = token;
        CreatedTime = DateTimeOffset.Now;
        ExpiresTime = CreatedTime.AddDays(1);
    }

    public UserId UserId { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset CreatedTime { get; init; }
    public DateTimeOffset ExpiresTime { get; init; }
    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

    public void Use()
    {
        if (IsUsed ||
            IsRevoked ||
            ExpiresTime < DateTimeOffset.Now)
            throw new KnownException("无效的刷新令牌", ErrorCodes.InvalidRefreshToken);

        IsUsed = true;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
