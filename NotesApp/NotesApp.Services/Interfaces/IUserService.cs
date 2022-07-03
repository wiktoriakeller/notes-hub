using NotesApp.Services.Dto;

namespace NotesApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<int> Register(CreateUserDto dto);
        Task<int> Register(RegisterUserDto dto);
        Task<string> Login(LoginDto dto);
        Task ForgotPassword(ForgotPasswordRequestDto dto);
        Task ResetPassword(ResetPasswordDto dto, string token);
    }
}