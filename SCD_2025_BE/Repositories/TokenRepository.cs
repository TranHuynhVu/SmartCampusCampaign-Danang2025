using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using SCD_2025_BE.Data;
using SCD_2025_BE.Entities.Domains;

namespace SCD_2025_BE.Repositories
{
    public class TokenRepository : ITokenReposity
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public TokenRepository(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> CreateJWTToken(AppUser user, string role)
        {
            //Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            if(role == "Company")
            {
                //lấy thông tin CompanyInfor để add claim
                var companyInfor = await _context.CompanyInfors
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                
                if(companyInfor != null)
                {
                    claims.Add(new Claim("CompanyId", companyInfor.Id.ToString()));
                    claims.Add(new Claim("CompanyName", companyInfor.CompanyName ?? ""));
                    claims.Add(new Claim("Descriptions", companyInfor.Descriptions ?? ""));
                    claims.Add(new Claim("CompanyWebsite", companyInfor.CompanyWebsite ?? ""));
                    claims.Add(new Claim("LogoUrl", companyInfor.LogoUrl ?? ""));
                    claims.Add(new Claim("Location", companyInfor.Location ?? ""));
                }
            }
            else if(role == "Student")
            {
                //lấy thông tin StudentInfor để add claim
                var studentInfor = await _context.StudentInfors
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                
                if(studentInfor != null)
                {
                    claims.Add(new Claim("StudentId", studentInfor.Id.ToString()));
                    claims.Add(new Claim("StudentName", studentInfor.Name ?? ""));
                    claims.Add(new Claim("ResumeUrl", studentInfor.ResumeUrl ?? ""));
                    claims.Add(new Claim("GPA", studentInfor.GPA ?? ""));
                    claims.Add(new Claim("Skills", studentInfor.Skills ?? ""));
                    claims.Add(new Claim("Archievements", studentInfor.Archievements ?? ""));
                    claims.Add(new Claim("YearOfStudy", studentInfor.YearOfStudy ?? ""));
                    claims.Add(new Claim("Major", studentInfor.Major ?? ""));
                    claims.Add(new Claim("Languages", studentInfor.Languages ?? ""));
                    claims.Add(new Claim("Certifications", studentInfor.Certifications ?? ""));
                    claims.Add(new Claim("Experiences", studentInfor.Experiences ?? ""));
                    claims.Add(new Claim("Projects", studentInfor.Projects ?? ""));
                    claims.Add(new Claim("Educations", studentInfor.Educations ?? ""));
                    claims.Add(new Claim("AvatarUrl", studentInfor.AvatarUrl ?? ""));
                    claims.Add(new Claim("OpenToWork", studentInfor.OpenToWork?.ToString() ?? "false"));
                }
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(AppUser user)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Convert.ToBase64String(randomNumber),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
