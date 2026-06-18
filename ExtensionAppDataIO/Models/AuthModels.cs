namespace ExtensionAppDataIO.Models
{
    public class AuthorizeRequest
    {
        public string redirect_uri { get; set; } = string.Empty;
        public string client_id { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
    }

    public class TokenRequest
    {
        public string grant_type { get; set; } = string.Empty;
        public string? code { get; set; }
        public string? refresh_token { get; set; }
        public string? client_id { get; set; }
        public string? client_secret { get; set; }
    }

    public class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string refresh_token { get; set; } = string.Empty;
    }

    public class UserInfoResponse
    {
        public string sub { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class AuthorizeViewModel
    {
        public string RedirectUri { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
