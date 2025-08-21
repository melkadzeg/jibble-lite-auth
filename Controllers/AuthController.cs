using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("")]
public class AuthController(IJwtTokenService tokens) : ControllerBase
{
    // super-simple in-memory users store for Day 1
    private static readonly Dictionary<string,(string Password,string CompanyId)> _users =
        new(StringComparer.OrdinalIgnoreCase);

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.CompanyId))
            return BadRequest("email, password, companyId required");

        if (_users.ContainsKey(dto.Email))
            return Conflict("user exists");

        _users[dto.Email] = (dto.Password, dto.CompanyId);
        return Created($"/users/{dto.Email}", new { dto.Email, dto.CompanyId });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        if (!_users.TryGetValue(dto.Email, out var rec) || rec.Password != dto.Password)
            return Unauthorized();

        var jwt = tokens.Issue(dto.Email, rec.CompanyId);
        return Ok(new { access_token = jwt });
    }

    [HttpGet("whoami")]
    [Authorize] // This endpoint now requires JWT authentication
    public IActionResult WhoAmI()
    {
        // Extract user info from JWT claims (parsed by ASP.NET Core JWT middleware)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        
        var companyId = User.FindFirst("companyId")?.Value;

        return Ok(new { via = "auth-service", userId, companyId });
    }
}