// Services/ITokenService.cs
using AuthService.Domain;

namespace AuthService.Services;

public interface ITokenService
{
    string CreateAccessToken(User user);
    (string Plain, string Hash, DateTimeOffset ExpiresAt) CreateRefreshToken();
    string Hash(string value);
}