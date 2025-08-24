// Repositories/IUserRepository.cs
using AuthService.Domain;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string emailNormalized, CancellationToken ct);
    Task<bool> EmailExistsAsync(string emailNormalized, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}