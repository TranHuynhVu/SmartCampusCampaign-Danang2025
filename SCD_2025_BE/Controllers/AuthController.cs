using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenReposity _tokenRepository;

        public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ITokenReposity token)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenRepository = token;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
        {
            // Validate role
            if (requestDto.Role != "Student" && requestDto.Role != "Company")
            {
                return BadRequest(new { Message = "Vai trò phải là Student hoặc Company." });
            }

            var user = new AppUser
            {
                UserName = requestDto.Email,
                Email = requestDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, requestDto.Password);

            if (result.Succeeded)
            {
                // Kiểm tra và tạo role nếu chưa có
                if (!await _roleManager.RoleExistsAsync(requestDto.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(requestDto.Role));
                }

                // Gán role cho user
                await _userManager.AddToRoleAsync(user, requestDto.Role);

                return Ok(new { Message = "Đăng ký thành công.", Role = requestDto.Role });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto)
        {
            var user = await _userManager.FindByEmailAsync(requestDto.Email);

            if (user != null)
            {
                var result = await _userManager.CheckPasswordAsync(user, requestDto.Password);

                if (result)
                {
                    var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                    var accessToken = await _tokenRepository.CreateJWTToken(user, role);

                    var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user);

                    await _tokenRepository.SaveRefreshTokenAsync(refreshToken);

                    var OAuth2Token = new OAuth2Token
                    {
                        access_token = accessToken,
                        refresh_token = refreshToken.Token,
                        token_type = "Bearer",
                        expires_in = 3600,
                        scope = role
                    };

                    return Ok(OAuth2Token);
                }
            }

            return BadRequest(new { Message = "Invalid email or password." });
        }

        [HttpPost]
        [Route("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var oldToken = await _tokenRepository.GetRefreshTokenAsync(request.Token);

            if (oldToken == null || oldToken.ExpiresAt <= DateTime.UtcNow || oldToken.IsRevoked == true)
                return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");

            // Sinh token mới
            var user = oldToken.User;
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Student";

            var accessToken = await _tokenRepository.CreateJWTToken(user, role);

            // Thu hồi token cũ
            await _tokenRepository.RevokeRefreshTokenAsync(request.Token);

            var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user);

            await _tokenRepository.SaveRefreshTokenAsync(refreshToken);


            var OAuth2Token = new OAuth2Token
            {
                access_token = accessToken,
                refresh_token = refreshToken.Token,
                token_type = "Bearer",
                expires_in = 3600,
                scope = role
            };

            return Ok(OAuth2Token);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            await _tokenRepository.RevokeRefreshTokenAsync(request.Token);
            return Ok(new { Message = "Đăng xuất thành công." });
        }

        [HttpGet]
        [Route("Test")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> Test()
        {
            return Ok("API is working!");
        }

    }
}
