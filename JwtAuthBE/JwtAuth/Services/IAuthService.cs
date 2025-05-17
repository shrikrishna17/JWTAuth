using JwtAuth.Dto;
using JwtAuth.Entities;

namespace JwtAuth.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto user);
        //Task<string?> LoginAsync(UserDto user);
        Task<TokenResponseDto?> LoginAsync(UserDto user);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto refreshTokenRequestDto);

    }
}
