// Services/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly int _accessMinutes;
    private readonly int _refreshDays;

    public TokenService(IConfiguration cfg)
    {
        _issuer = cfg["Jwt:Issuer"]!;
        _audience = cfg["Jwt:Audience"]!;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
        _accessMinutes = int.TryParse(cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 15;
        _refreshDays = int.TryParse(cfg["Jwt:RefreshTokenDays"], out var d) ? d : 60;
    }

    public string CreateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.DisplayName ?? user.Email)
        };

        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_accessMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public (string Plain, string Hash, DateTimeOffset ExpiresAt) CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32); // 256-bit
        var plain = Convert.ToBase64String(bytes);
        var hash = Hash(plain);
        var exp = DateTimeOffset.UtcNow.AddDays(_refreshDays);
        return (plain, hash, exp);
    }

    public string Hash(string value)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes); // uppercase hex
    }
}
