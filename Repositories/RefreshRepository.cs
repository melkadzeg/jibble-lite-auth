// Repositories/RefreshTokenRepository.cs
using AuthService.Data;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _db;
    public RefreshTokenRepository(AuthDbContext db) => _db = db;

    public Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct) =>
        _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash
                                      && r.RevokedAt == null
                                      && r.ExpiresAt > DateTimeOffset.UtcNow, ct);

    public Task AddAsync(RefreshToken token, CancellationToken ct) =>
        _db.RefreshTokens.AddAsync(token, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}