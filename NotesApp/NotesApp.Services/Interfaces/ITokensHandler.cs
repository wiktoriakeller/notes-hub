using NotesApp.Domain.Entities;

namespace NotesApp.Services.Interfaces
{
    public interface ITokensHandler
    {
        Task<string> GenerateJwtToken(User user);
        Task<RefreshToken> GenerateRefreshToken();
        string GenerateRandomToken();
    }
}
