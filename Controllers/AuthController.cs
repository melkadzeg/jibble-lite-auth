// Controllers/AuthController.cs
using System.Security.Claims;
using AuthService.Contracts;
using AuthService.Domain;
using AuthService.Repositories;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refresh;
    private readonly ITokenService _tokens;
    private readonly PasswordHasher<User> _hasher = new();

    public AuthController(IUserRepository users, IRefreshTokenRepository refresh, ITokenService tokens)
    {
        _users = users;
        _refresh = refresh;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password required.");

        if (await _users.EmailExistsAsync(email, ct))
            return Conflict("Email already registered.");

        var user = new User
        {
            Email = email,
            DisplayName = req.DisplayName?.Trim()
        };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        return Created($"/auth/users/{user.Id}", new { user.Id, user.Email, user.DisplayName });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenPairResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);
        if (user is null) return Unauthorized("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

        var access = _tokens.CreateAccessToken(user);
        var (plain, hash, exp) = _tokens.CreateRefreshToken();

        await _refresh.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAt = exp
        }, ct);
        await _refresh.SaveChangesAsync(ct);

        return new TokenPairResponse(access, plain);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenPairResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return BadRequest("Missing refresh token.");

        var hash = _tokens.Hash(req.RefreshToken);
        var token = await _refresh.GetActiveByHashAsync(hash, ct);
        if (token is null) return Unauthorized("Invalid or expired refresh token.");

        // rotate: revoke old, issue new
        token.RevokedAt = DateTimeOffset.UtcNow;
        var (plain, newHash, exp) = _tokens.CreateRefreshToken();
        var user = token.User;

        await _refresh.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = exp,
            ReplacedByTokenHash = newHash
        }, ct);

        var access = _tokens.CreateAccessToken(user);
        await _refresh.SaveChangesAsync(ct);

        return new TokenPairResponse(access, plain);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken)) return BadRequest("Missing refresh token.");
        var hash = _tokens.Hash(req.RefreshToken);
        var token = await _refresh.GetActiveByHashAsync(hash, ct);
        if (token is null) return NoContent();

        token.RevokedAt = DateTimeOffset.UtcNow;
        await _refresh.SaveChangesAsync(ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!long.TryParse(sub, out var uid)) return Unauthorized();

        var user = await _users.GetByIdAsync(uid, ct);
        if (user is null) return Unauthorized();

        return new MeResponse(user.Id, user.Email, user.DisplayName);
    }
}
