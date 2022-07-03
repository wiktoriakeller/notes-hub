namespace NotesApp.Services.Authorization
{
    public class AuthenticationSettings
    {
        public string Secret { get; set; }
        public int JwtTokenExpireMinutes { get; set; }
        public int RefreshTokenExpireDays { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
