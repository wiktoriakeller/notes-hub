using Microsoft.AspNetCore.Mvc;
using NotesApp.Services.Interfaces;
using NotesApp.Services.Dto;
using Microsoft.AspNetCore.Authorization;

namespace NotesApp.WebAPI.Controllers
{
    [Route("notes-api/accounts")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _usersService;

        public UserController(IUserService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            await _usersService.Register(dto);
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            await _usersService.Register(dto);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string token = await _usersService.Login(dto);
            return Ok(token);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            await _usersService.ForgotPassword(dto);
            return Ok();
        }

        [HttpPost("reset-password/{token}")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, [FromRoute] string token)
        {
            await _usersService.ResetPassword(dto, token);
            return Ok();
        }

    }
}
