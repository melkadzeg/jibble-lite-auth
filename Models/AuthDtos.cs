namespace AuthService.Models;

public record RegisterDto(string Email, string Password, string TenantId);
public record LoginDto(string Email, string Password);