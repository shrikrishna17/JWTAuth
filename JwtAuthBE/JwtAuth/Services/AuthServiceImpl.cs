using Azure.Core;
using JwtAuth.Data;
using JwtAuth.Dto;
using JwtAuth.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace JwtAuth.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;
        public AuthServiceImpl(DataContext dataContext, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _configuration = configuration;
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if(await _dataContext.Users.AnyAsync(u => u.Username == request.UserName))
            {
                return null;
            }
            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Username = request.UserName;
            user.PasswordHash = hashedPassword;
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            return user;
        }
        //public async Task<string?> LoginAsync(UserDto request)
        //{
        //    var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.UserName);

        //    if (user is null)
        //    {
        //        return null;
        //    }
        //    if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
        //        == PasswordVerificationResult.Failed)
        //    {
        //        return null;
        //    }

        //    return CreateToken(user);
        //}
        public async Task<TokenResponseDto?> LoginAsync(UserDto request)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.UserName);

            if (user is null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }
           
            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(1);
            await _dataContext.SaveChangesAsync();
            return refreshToken;
        } 

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _dataContext.Users.FindAsync(userId);
            if(user is null || user.RefreshToken != refreshToken 
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if(user is null)
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }
    }
}
