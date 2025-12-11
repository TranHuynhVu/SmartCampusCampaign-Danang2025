using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;
using System.Security.Claims;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyInforsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyInforsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CompanyInforResponseDto>>> GetCompanyInfors()
        {
            var companies = await _unitOfWork.CompanyInfors.GetActiveCompaniesAsync();

            var response = companies.Select(c => new CompanyInforResponseDto
            {
                Id = c.Id,
                UserId = c.UserId,
                CompanyName = c.CompanyName,
                Descriptions = c.Descriptions,
                CompanyWebsite = c.CompanyWebsite,
                LogoUrl = c.LogoUrl,
                Location = c.Location,
                Description = c.Description,
                UpdatedBy = c.UpdatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CompanyInforResponseDto>> GetCompanyInfor(int id)
        {
            var company = await _unitOfWork.CompanyInfors.GetByIdAsync(id);

            if (company == null || company.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin công ty." });
            }

            var response = new CompanyInforResponseDto
            {
                Id = company.Id,
                UserId = company.UserId,
                CompanyName = company.CompanyName,
                Descriptions = company.Descriptions,
                CompanyWebsite = company.CompanyWebsite,
                LogoUrl = company.LogoUrl,
                Location = company.Location,
                Description = company.Description,
                UpdatedBy = company.UpdatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

            return Ok(response);
        }

        [HttpGet("MyCompany")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<CompanyInforResponseDto>> GetMyCompany()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var company = await _unitOfWork.CompanyInfors.GetByUserIdAsync(userId);

            if (company == null)
            {
                return NotFound(new { Message = "Bạn chưa tạo thông tin công ty." });
            }

            var response = new CompanyInforResponseDto
            {
                Id = company.Id,
                UserId = company.UserId,
                CompanyName = company.CompanyName,
                Descriptions = company.Descriptions,
                CompanyWebsite = company.CompanyWebsite,
                LogoUrl = company.LogoUrl,
                Location = company.Location,
                Description = company.Description,
                UpdatedBy = company.UpdatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Company")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CompanyInforResponseDto>> CreateCompanyInfor([FromForm] CompanyInforDto companyDto, IFormFile? logoFile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingCompany = await _unitOfWork.CompanyInfors.GetByUserIdAsync(userId);

            // Nếu đã có thông tin công ty, chuyển sang update
            if (existingCompany != null)
            {
                return await UpdateExistingCompany(existingCompany, companyDto, logoFile, userId);
            }

            // Xử lý file upload
            string? logoUrl = null;

            if (logoFile != null)
            {
                // Kiểm tra file có phải là hình ảnh không
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(logoFile.ContentType))
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF) cho logo." });
                }

                // Kiểm tra kích thước file (giới hạn 2MB)
                if (logoFile.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File logo không được vượt quá 2MB." });
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(logoFile.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                logoUrl = $"/logos/{fileName}";
            }
            else if (!string.IsNullOrEmpty(companyDto.LogoUrl))
            {
                // Nếu không có file upload, dùng URL từ DTO
                logoUrl = companyDto.LogoUrl;
            }

            var company = new CompanyInfor
            {
                UserId = userId,
                CompanyName = companyDto.CompanyName,
                Descriptions = companyDto.Descriptions,
                CompanyWebsite = companyDto.CompanyWebsite,
                LogoUrl = logoUrl,
                Location = companyDto.Location,
                Description = companyDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.CompanyInfors.AddAsync(company);
            await _unitOfWork.SaveChangesAsync();

            var response = new CompanyInforResponseDto
            {
                Id = company.Id,
                UserId = company.UserId,
                CompanyName = company.CompanyName,
                Descriptions = company.Descriptions,
                CompanyWebsite = company.CompanyWebsite,
                LogoUrl = company.LogoUrl,
                Location = company.Location,
                Description = company.Description,
                UpdatedBy = company.UpdatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCompanyInfor), new { id = company.Id }, response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> DeleteCompanyInfor(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var company = await _unitOfWork.CompanyInfors.GetByIdAsync(id);

            if (company == null || company.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin công ty." });
            }

            if (userRole != "Admin" && company.UserId != userId)
            {
                return Forbid();
            }

            company.DeletedAt = DateTime.UtcNow;
            company.UpdatedBy = userId;

            _unitOfWork.CompanyInfors.Update(company);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // Helper method để xử lý update khi company đã tồn tại
        private async Task<ActionResult<CompanyInforResponseDto>> UpdateExistingCompany(
            CompanyInfor company, 
            CompanyInforDto companyDto, 
            IFormFile? logoFile, 
            string userId)
        {
            // Xử lý file upload
            if (logoFile != null)
            {
                // Kiểm tra file có phải là hình ảnh không
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(logoFile.ContentType))
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF) cho logo." });
                }

                // Kiểm tra kích thước file (giới hạn 2MB)
                if (logoFile.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File logo không được vượt quá 2MB." });
                }

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(company.LogoUrl) && company.LogoUrl.StartsWith("/logos/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", company.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(logoFile.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                company.LogoUrl = $"/logos/{fileName}";
            }
            else if (!string.IsNullOrEmpty(companyDto.LogoUrl))
            {
                // Nếu không có file upload, dùng URL từ DTO
                company.LogoUrl = companyDto.LogoUrl;
            }

            company.CompanyName = companyDto.CompanyName;
            company.Descriptions = companyDto.Descriptions;
            company.CompanyWebsite = companyDto.CompanyWebsite;
            company.Location = companyDto.Location;
            company.Description = companyDto.Description;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = userId;

            _unitOfWork.CompanyInfors.Update(company);
            await _unitOfWork.SaveChangesAsync();

            var response = new CompanyInforResponseDto
            {
                Id = company.Id,
                UserId = company.UserId,
                CompanyName = company.CompanyName,
                Descriptions = company.Descriptions,
                CompanyWebsite = company.CompanyWebsite,
                LogoUrl = company.LogoUrl,
                Location = company.Location,
                Description = company.Description,
                UpdatedBy = company.UpdatedBy,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };

            return Ok(response);
        }
    }
}
