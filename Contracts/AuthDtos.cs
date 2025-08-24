namespace AuthService.Contracts;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record TokenPairResponse(string AccessToken, string RefreshToken);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record MeResponse(long Id, string Email, string? DisplayName);