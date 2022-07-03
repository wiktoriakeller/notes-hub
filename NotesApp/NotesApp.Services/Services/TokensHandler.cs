using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using NotesApp.Services.Authorization;
using NotesApp.Services.Interfaces;
using System.Security.Cryptography;
using NotesApp.Domain.Entities;
using System.Security.Claims;
using System.Text;

namespace NotesApp.Services.Services
{
    public class TokensHandler : ITokensHandler
    {
        private readonly AuthenticationSettings _authenticationSettings;

        public TokensHandler(AuthenticationSettings authenticationSettings)
        {
            _authenticationSettings = authenticationSettings;
        }

        public Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.Secret));
            var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_authenticationSettings.JwtTokenExpireMinutes);
            var notBefore = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                _authenticationSettings.Issuer,
                _authenticationSettings.Audience,
                claims,
                notBefore: notBefore,
                expires: expires,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return Task.FromResult(tokenHandler.WriteToken(token));
        }

        public Task<RefreshToken> GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomToken(),
                Expires = DateTime.UtcNow.AddDays(_authenticationSettings.RefreshTokenExpireDays),
                Created = DateTime.UtcNow
            };

            return Task.FromResult(refreshToken);
        }

        public string GenerateRandomToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
