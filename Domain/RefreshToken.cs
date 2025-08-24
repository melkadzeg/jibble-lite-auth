namespace AuthService.Domain;

public class RefreshToken
{
    public long Id { get; set; }   // long PK, autoincrement

    public long UserId { get; set; }   // FK → Users.Id
    public User User { get; set; } = default!;

    public string TokenHash { get; set; } = default!;

    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
}