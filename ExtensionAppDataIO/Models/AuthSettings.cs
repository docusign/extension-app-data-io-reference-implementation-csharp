namespace ExtensionAppDataIO.Models
{
    public class AuthSettings
    {
        public string JwtSecretKey { get; set; } = string.Empty;
        public string OAuthClientId { get; set; } = string.Empty;
        public string OAuthClientSecret { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
    }
}
