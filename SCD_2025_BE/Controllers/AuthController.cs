using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;
using SCD_2025_BE.Service;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenReposity _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGemini _geminiService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            UserManager<AppUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            ITokenReposity token, 
            IUnitOfWork unitOfWork,
            IGemini geminiService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenRepository = token;
            _unitOfWork = unitOfWork;
            _geminiService = geminiService;
            _environment = environment;
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

        [HttpPost]
        [Route("RegisterStudent")]
        public async Task<IActionResult> RegisterStudent([FromForm] RegisterStudentDto requestDto)
        {
            StudentInfor studentInfor = null;
            string resumeFileName = null;

            // Nếu có resume PDF, đọc và trích xuất dữ liệu
            if (requestDto.Resume != null && requestDto.Resume.Length > 0)
            {
                // Validate file type
                if (requestDto.Resume.ContentType != "application/pdf")
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file PDF." });
                }

                // Validate file size (max 10MB)
                if (requestDto.Resume.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "Kích thước file không được vượt quá 10MB." });
                }

                try
                {
                    // Đọc file PDF
                    byte[] pdfBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await requestDto.Resume.CopyToAsync(memoryStream);
                        pdfBytes = memoryStream.ToArray();
                    }

                    // Trích xuất thông tin từ PDF bằng Gemini
                    var extractedInfo = await _geminiService.ClassifyStudentCV(pdfBytes);
                    
                    if (extractedInfo != null)
                    {
                        studentInfor = extractedInfo;
                        
                        // Nếu có dữ liệu nhập tay, ưu tiên dữ liệu nhập tay cho các trường không null
                        if (!string.IsNullOrEmpty(requestDto.Name))
                            studentInfor.Name = requestDto.Name;
                        if (!string.IsNullOrEmpty(requestDto.GPA))
                            studentInfor.GPA = requestDto.GPA;
                        if (!string.IsNullOrEmpty(requestDto.Skills))
                            studentInfor.Skills = requestDto.Skills;
                        if (!string.IsNullOrEmpty(requestDto.Archievements))
                            studentInfor.Archievements = requestDto.Archievements;
                        if (!string.IsNullOrEmpty(requestDto.YearOfStudy))
                            studentInfor.YearOfStudy = requestDto.YearOfStudy;
                        if (!string.IsNullOrEmpty(requestDto.Major))
                            studentInfor.Major = requestDto.Major;
                        if (!string.IsNullOrEmpty(requestDto.Languages))
                            studentInfor.Languages = requestDto.Languages;
                        if (!string.IsNullOrEmpty(requestDto.Certifications))
                            studentInfor.Certifications = requestDto.Certifications;
                        if (!string.IsNullOrEmpty(requestDto.Experiences))
                            studentInfor.Experiences = requestDto.Experiences;
                        if (!string.IsNullOrEmpty(requestDto.Projects))
                            studentInfor.Projects = requestDto.Projects;
                        if (!string.IsNullOrEmpty(requestDto.Educations))
                            studentInfor.Educations = requestDto.Educations;

                        studentInfor.OpenToWork = true; // Mặc định OpenToWork = true
                    }
                    else
                    {
                        return BadRequest(new { Message = "Không thể trích xuất thông tin từ CV. Vui lòng nhập thông tin thủ công." });
                    }

                    // Lưu file PDF
                    var resumesFolder = Path.Combine(_environment.WebRootPath, "resumes");
                    if (!Directory.Exists(resumesFolder))
                    {
                        Directory.CreateDirectory(resumesFolder);
                    }

                    resumeFileName = $"{Guid.NewGuid()}_{requestDto.Resume.FileName}";
                    var filePath = Path.Combine(resumesFolder, resumeFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await requestDto.Resume.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = $"Lỗi khi xử lý CV: {ex.Message}" });
                }
            }
            else
            {
                // Không có resume, sử dụng dữ liệu nhập tay
                if (string.IsNullOrEmpty(requestDto.Name))
                {
                    return BadRequest(new { Message = "Tên sinh viên là bắt buộc khi không có CV." });
                }

                studentInfor = new StudentInfor
                {
                    Name = requestDto.Name,
                    GPA = requestDto.GPA,
                    Skills = requestDto.Skills,
                    Archievements = requestDto.Archievements,
                    YearOfStudy = requestDto.YearOfStudy,
                    Major = requestDto.Major,
                    Languages = requestDto.Languages,
                    Certifications = requestDto.Certifications,
                    Experiences = requestDto.Experiences,
                    Projects = requestDto.Projects,
                    Educations = requestDto.Educations
                };
            }

            // Tạo user
            var user = new AppUser
            {
                UserName = requestDto.Email,
                Email = requestDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, requestDto.Password);

            if (!result.Succeeded)
            {
                // Nếu tạo user thất bại, xóa file resume đã upload
                if (!string.IsNullOrEmpty(resumeFileName))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "resumes", resumeFileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                return BadRequest(new { Message = "Không thể tạo tài khoản.", Errors = result.Errors });
            }

            // Tạo và gán role Student
            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Student"));
            }
            await _userManager.AddToRoleAsync(user, "Student");

            // Gán UserId và các thông tin khác
            studentInfor.UserId = user.Id;
            studentInfor.OpenToWork = true; // Mặc định OpenToWork = true
            studentInfor.CreatedAt = DateTime.UtcNow;
            
            // Lưu đường dẫn resume nếu có
            if (!string.IsNullOrEmpty(resumeFileName))
            {
                studentInfor.ResumeUrl = $"/resumes/{resumeFileName}";
            }

            // Tạo embedding cho hồ sơ sinh viên
            try
            {
                var embeddingString = await _geminiService.GetStudentEmbedding(studentInfor);
                studentInfor.EmBeddings = embeddingString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not generate embedding: {ex.Message}");
                // Không fail toàn bộ quá trình nếu embedding thất bại
            }

            await _unitOfWork.StudentInfors.AddAsync(studentInfor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { 
                Message = "Đăng ký sinh viên thành công.", 
                UserId = user.Id,
                ResumeProcessed = requestDto.Resume != null
            });
        }

        [HttpPost]
        [Route("RegisterCompany")]
        public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyDto requestDto)
        {
            // Tạo user
            var user = new AppUser
            {
                UserName = requestDto.Email,
                Email = requestDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, requestDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Không thể tạo tài khoản.", Errors = result.Errors });
            }

            // Tạo và gán role Company
            if (!await _roleManager.RoleExistsAsync("Company"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Company"));
            }
            await _userManager.AddToRoleAsync(user, "Company");

            // Tạo CompanyInfor
            var companyInfor = new CompanyInfor
            {
                UserId = user.Id,
                CompanyName = requestDto.CompanyName,
                CompanyWebsite = requestDto.CompanyWebsite,
                Location = requestDto.Location,
                Descriptions = requestDto.Descriptions,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CompanyInfors.AddAsync(companyInfor);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký công ty thành công.", UserId = user.Id });
        }

    }
}
