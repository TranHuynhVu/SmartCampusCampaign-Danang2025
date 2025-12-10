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
        public async Task<ActionResult<CompanyInforResponseDto>> CreateCompanyInfor(CompanyInforDto companyDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingCompany = await _unitOfWork.CompanyInfors.GetByUserIdAsync(userId);

            if (existingCompany != null)
            {
                return BadRequest(new { Message = "Bạn đã tạo thông tin công ty rồi." });
            }

            var company = new CompanyInfor
            {
                UserId = userId,
                CompanyName = companyDto.CompanyName,
                Descriptions = companyDto.Descriptions,
                CompanyWebsite = companyDto.CompanyWebsite,
                LogoUrl = companyDto.LogoUrl,
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateCompanyInfor(int id, CompanyInforDto companyDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var company = await _unitOfWork.CompanyInfors.GetByIdAsync(id);

            if (company == null || company.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin công ty." });
            }

            if (company.UserId != userId)
            {
                return Forbid();
            }

            company.CompanyName = companyDto.CompanyName;
            company.Descriptions = companyDto.Descriptions;
            company.CompanyWebsite = companyDto.CompanyWebsite;
            company.LogoUrl = companyDto.LogoUrl;
            company.Location = companyDto.Location;
            company.Description = companyDto.Description;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedBy = userId;

            _unitOfWork.CompanyInfors.Update(company);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
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
    }
}
