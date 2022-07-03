using NotesApp.Services.Interfaces;
using NotesApp.Domain.Entities;
using NotesApp.Domain.Interfaces;
using NotesApp.Services.Dto;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using NotesApp.Services.Exceptions;
using NotesApp.Services.Email;
using System.Text;
using AutoMapper;

namespace NotesApp.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ITokensHandler _tokenHandler;
        private readonly EmailSettings _emailSettings;

        public UserService(
            IUserRepository usersRepository, 
            IPasswordHasher<User> passwordHasher, 
            IMapper mapper,
            IEmailService emailService,
            ITokensHandler tokenHandler,
            EmailSettings emailSettings)
        {
            _userRepository = usersRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _emailService = emailService;
            _tokenHandler = tokenHandler;
            _emailSettings = emailSettings;
        }

        public async Task<int> Register(CreateUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            var hashedPassword = _passwordHasher.HashPassword(user, dto.Password);
            user.PasswordHash = hashedPassword;

            await _userRepository.AddAsync(user);
            return user.Id;
        }

        public async Task<int> Register(RegisterUserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.RoleId = 2;

            var hashedPassword = _passwordHasher.HashPassword(user, dto.Password);
            user.PasswordHash = hashedPassword;

            await _userRepository.AddAsync(user);
            return user.Id;
        }

        public async Task<string> Login(LoginDto dto)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Login == dto.Login, "Role");
            var token = await _tokenHandler.GenerateJwtToken(user);
            return token;
        }

        public async Task ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == dto.Email);
            
            if(user == null)
                throw new NotFoundException($"User with email: {dto.Email} doesn't exists");

            var token = _tokenHandler.GenerateRandomToken();
            var tokenHash = ComputeHash(token);
            var expiredDate = DateTimeOffset.Now.AddHours(2);

            try
            {
                await _emailService.SendEmailAsync(new EmailMessage()
                {
                    To = dto.Email,
                    Subject = "Notes reset password",
                    Content = $"Click here to reset you password: {_emailSettings.ResetPasswordRoute}{token}\nThis link is only valid till: {expiredDate}"
                });
            }
            catch(Exception e)
            {
                throw new InternalServerErrorException("An email couldn't be send");
            }

            user.ResetToken = tokenHash;
            user.ResetTokenExpires = expiredDate;

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPassword(ResetPasswordDto dto, string token)
        {
            var tokenHash = ComputeHash(token);
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.ResetToken == tokenHash);

            if (user == null || user.ResetTokenExpires < DateTimeOffset.Now)
                throw new BadRequestException("Invalid or expired link");

            user.ResetToken = null;
            user.ResetTokenExpires = null;
            var hashedPassword = _passwordHasher.HashPassword(user, dto.Password);
            user.PasswordHash = hashedPassword;
            await _userRepository.UpdateAsync(user);
        }

        private string ComputeHash(string data)
        {
            using var sha256 = SHA256.Create();
            var tokenHash = string.Join("", sha256.ComputeHash(Encoding.UTF8.GetBytes(data)).Select(x => x.ToString("x2")));
            return tokenHash;
        }
    }
}
