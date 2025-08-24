// Repositories/IRefreshTokenRepository.cs
using AuthService.Domain;

namespace AuthService.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(RefreshToken token, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}