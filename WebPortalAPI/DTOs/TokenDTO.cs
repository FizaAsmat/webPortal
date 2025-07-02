namespace WebPortalAPI.DTOs
{
    public class TokenDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }

    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; }
    }
} 