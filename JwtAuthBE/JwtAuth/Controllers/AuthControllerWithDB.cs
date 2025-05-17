using JwtAuth.Dto;
using JwtAuth.Entities;
using JwtAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthControllerWithDB : ControllerBase
    {
        public static User user = new();
        private readonly IAuthService _authService;
        public AuthControllerWithDB(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if(user == null)
            {
                return BadRequest("UserName already exists");
            }
            return Ok(user);
        }
        //[HttpPost("login")]
        //public async Task<ActionResult<string>> login(UserDto request)
        //{
        //    var token = await _authService.LoginAsync(request);
        //    if (token is null)
        //    {
        //        return BadRequest("Invalid username or password");
        //    }
        //    return Ok(token);
        //}

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> login(UserDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid username or password");
            }
            return Ok(result);
        }
        [Authorize]
        [HttpGet]
        public IActionResult AuthenticateOnlyEndPoint()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndPoiny()
        {
            return Ok("You are Admin!!!");
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken == null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh Token");
            }
            return Ok(result);
        }

    }
}