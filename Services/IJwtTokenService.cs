namespace AuthService.Services;
public interface IJwtTokenService
{
    string Issue(string userId, string companyId);
}