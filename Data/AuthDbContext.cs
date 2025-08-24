using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .ValueGeneratedOnAdd(); // autoincrement

            e.Property(x => x.Email).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.DisplayName);
            e.Property(x => x.Status).HasDefaultValue((short)1);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.LastLoginAt);
        });

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            e.Property(x => x.UserId).IsRequired();
            e.Property(x => x.TokenHash).IsRequired();

            e.Property(x => x.IssuedAt).HasDefaultValueSql("now()");
            e.Property(x => x.ExpiresAt).IsRequired();
            e.Property(x => x.RevokedAt);
            e.Property(x => x.ReplacedByTokenHash);

            e.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.UserId, x.ExpiresAt })
                .HasDatabaseName("IX_RefreshTokens_Active");
        });
    }
}