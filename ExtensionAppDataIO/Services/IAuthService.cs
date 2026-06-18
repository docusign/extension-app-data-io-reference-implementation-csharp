using ExtensionAppDataIO.Models;

namespace ExtensionAppDataIO.Services
{
    public interface IAuthService
    {
        TokenResponse GenerateTokenFromAuthCode(string code);
        TokenResponse GenerateTokenFromRefreshToken(string refreshToken);
        TokenResponse GenerateTokenFromClientCredentials(string clientId, string clientSecret);
    }
}
