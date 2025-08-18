namespace AuthService.Models;

public record RegisterDto(string Email, string Password, string ClientId);
public record LoginDto(string Email, string Password);