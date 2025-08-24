// Repositories/UserRepository.cs
using AuthService.Data;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;
    public UserRepository(AuthDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(long id, CancellationToken ct) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string emailNormalized, CancellationToken ct) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized, ct);

    public Task<bool> EmailExistsAsync(string emailNormalized, CancellationToken ct) =>
        _db.Users.AnyAsync(u => u.Email == emailNormalized, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _db.Users.AddAsync(user, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}