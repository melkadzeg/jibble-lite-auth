namespace AuthService.Models;

public record RegisterDto(string Email, string Password, string CompanyId);
public record LoginDto(string Email, string Password);