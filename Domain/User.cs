namespace AuthService.Domain;

public class User
{
    public long Id { get; set; }   // long PK, autoincrement

    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? DisplayName { get; set; }

    /// <summary>1 = Active, 0 = Disabled</summary>
    public short Status { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}