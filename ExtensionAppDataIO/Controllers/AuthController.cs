using ExtensionAppDataIO.Models;
using ExtensionAppDataIO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ExtensionAppDataIO.Controllers
{
    [Route("api/oauth")]
    public class AuthController : Controller
    {
        private enum OAuthGrantType
        {
            AuthorizationCode,
            RefreshToken,
            ClientCredentials
        }

        private readonly IAuthService _authService;
        private readonly AuthSettings _settings;

        public AuthController(IAuthService authService, IOptions<AuthSettings> options)
        {
            _authService = authService;
            _settings = options.Value;
        }

        private static bool TryParseGrantType(string? grantType, out OAuthGrantType parsedGrantType)
        {
            parsedGrantType = default;

            if (string.IsNullOrWhiteSpace(grantType))
                return false;

            switch (grantType.Trim().ToLowerInvariant())
            {
                case "authorization_code":
                    parsedGrantType = OAuthGrantType.AuthorizationCode;
                    return true;
                case "refresh_token":
                    parsedGrantType = OAuthGrantType.RefreshToken;
                    return true;
                case "client_credentials":
                    parsedGrantType = OAuthGrantType.ClientCredentials;
                    return true;
                default:
                    return false;
            }
        }

        // GET /api/oauth/authorize
        [HttpGet("authorize")]
        public IActionResult Authorize([FromQuery] AuthorizeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.client_id) || request.client_id != _settings.OAuthClientId)
                return BadRequest(new { error = "invalid_client" });

            if (!Uri.TryCreate(request.redirect_uri, UriKind.Absolute, out _))
                return BadRequest(new { error = "invalid_redirect_uri" });

            if (string.IsNullOrWhiteSpace(request.state))
                return BadRequest(new { error = "invalid_state" });

            return View(new AuthorizeViewModel
            {
                RedirectUri = request.redirect_uri,
                Code = _settings.AuthorizationCode,
                State = request.state,
            });
        }

        // POST /api/oauth/token
        [HttpPost("token")]
        public IActionResult Token([FromForm] TokenRequest request)
        {
            try
            {
                TokenResponse tokenResponse;

                if (!TryParseGrantType(request.grant_type, out var grantType))
                    return BadRequest(new { error = "unsupported_grant_type" });

                if (grantType == OAuthGrantType.AuthorizationCode)
                {
                    if (string.IsNullOrEmpty(request.code))
                        return BadRequest(new { error = "invalid_request", error_description = "code is required" });

                    tokenResponse = _authService.GenerateTokenFromAuthCode(request.code);
                }
                else if (grantType == OAuthGrantType.RefreshToken)
                {
                    if (string.IsNullOrEmpty(request.refresh_token))
                        return BadRequest(new { error = "invalid_request", error_description = "refresh_token is required" });

                    tokenResponse = _authService.GenerateTokenFromRefreshToken(request.refresh_token);
                }
                else
                {
                    string? clientId = null;
                    string? clientSecret = null;

                    var authHeader = Request.Headers.Authorization.ToString();
                    if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        var base64 = authHeader[6..];
                        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                        var parts = decoded.Split(':', 2);
                        clientId = parts[0];
                        clientSecret = parts.Length > 1 ? parts[1] : null;
                    }

                    if (clientId is null || clientSecret is null)
                        return Unauthorized(new { error = "invalid_client" });

                    tokenResponse = _authService.GenerateTokenFromClientCredentials(clientId, clientSecret);
                }

                return Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "invalid_grant" });
            }
        }

        // GET /api/oauth/userinfo  (JWT Bearer required)
        [HttpGet("userinfo")]
        [Authorize]
        public IActionResult UserInfo()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst("sub")?.Value
                      ?? string.Empty;
            var email = User.FindFirst("email")?.Value ?? string.Empty;

            return Ok(new UserInfoResponse { sub = sub, email = email });
        }
    }
}
