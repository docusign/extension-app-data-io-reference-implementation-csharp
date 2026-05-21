using ExtensionAppDataIO.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExtensionAppDataIO.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthSettings _settings;

        public AuthService(IOptions<AuthSettings> options)
        {
            _settings = options.Value;
        }

        private TokenResponse CreateTokenResponse()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var sub = Guid.NewGuid().ToString();
            var email = $"{Guid.NewGuid()}@test.com";

            var accessClaims = new[]
            {
                new Claim("type", "access_token"),
                new Claim(JwtRegisteredClaimNames.Sub, sub),
                new Claim("email", email),
            };

            var accessToken = new JwtSecurityToken(
                claims: accessClaims,
                expires: DateTime.UtcNow.AddSeconds(3600),
                signingCredentials: creds);

            var refreshClaims = new[]
            {
                new Claim("type", "refresh_token"),
            };

            var refreshToken = new JwtSecurityToken(
                claims: refreshClaims,
                signingCredentials: creds);

            var handler = new JwtSecurityTokenHandler();

            return new TokenResponse
            {
                access_token = handler.WriteToken(accessToken),
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = handler.WriteToken(refreshToken),
            };
        }

        public TokenResponse GenerateTokenFromAuthCode(string code)
        {
            var decodedCode = Uri.UnescapeDataString(code.Replace("+", "%20"));
            if (decodedCode != _settings.AuthorizationCode)
                throw new UnauthorizedAccessException("Invalid authorization code");

            return CreateTokenResponse();
        }

        public TokenResponse GenerateTokenFromRefreshToken(string refreshToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecretKey));
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
            };

            var principal = handler.ValidateToken(refreshToken, validationParams, out _);
            var typeClaim = principal.FindFirst("type")?.Value;
            if (typeClaim != "refresh_token")
                throw new UnauthorizedAccessException("Invalid refresh token type");

            return CreateTokenResponse();
        }

        public TokenResponse GenerateTokenFromClientCredentials(string clientId, string clientSecret)
        {
            var decodedClientId = Uri.UnescapeDataString(clientId.Replace("+", "%20"));
            var decodedClientSecret = Uri.UnescapeDataString(clientSecret.Replace("+", "%20"));

            if (decodedClientId != _settings.OAuthClientId || decodedClientSecret != _settings.OAuthClientSecret)
                throw new UnauthorizedAccessException("Invalid client credentials");

            return CreateTokenResponse();
        }
    }
}
