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
    public class JobsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public JobsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Jobs
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetJobs(
            [FromQuery] string? status = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? location = null)
        {
            var jobs = await _unitOfWork.Jobs.GetActiveJobsAsync(status, categoryId, location);

            var response = jobs.Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                SalaryRange = j.SalaryRange,
                DayOfWeek = j.DayOfWeek,
                TimeOfDay = j.TimeOfDay,
                Benefits = j.Benefits,
                Requirements = j.Requirements,
                NiceToHave = j.NiceToHave,
                CompanyInforId = j.CompanyInforId,
                CompanyName = j.CompanyInfor.CompanyName,
                Location = j.Location,
                Status = j.Status,
                CategoryId = j.CategoryId,
                CategoryName = j.Category.Name,
                UpdatedBy = j.UpdatedBy,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt
            });

            return Ok(response);
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JobResponseDto>> GetJob(int id)
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(id);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            var response = new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                SalaryRange = job.SalaryRange,
                DayOfWeek = job.DayOfWeek,
                TimeOfDay = job.TimeOfDay,
                Benefits = job.Benefits,
                Requirements = job.Requirements,
                NiceToHave = job.NiceToHave,
                CompanyInforId = job.CompanyInforId,
                CompanyName = job.CompanyInfor.CompanyName,
                Location = job.Location,
                Status = job.Status,
                CategoryId = job.CategoryId,
                CategoryName = job.Category.Name,
                UpdatedBy = job.UpdatedBy,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            };

            return Ok(response);
        }

        // GET: api/Jobs/MyJobs
        [HttpGet("MyJobs")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<JobResponseDto>>> GetMyJobs()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var jobs = await _unitOfWork.Jobs.GetJobsByUserIdAsync(userId);

            var response = jobs.Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                SalaryRange = j.SalaryRange,
                DayOfWeek = j.DayOfWeek,
                TimeOfDay = j.TimeOfDay,
                Benefits = j.Benefits,
                Requirements = j.Requirements,
                NiceToHave = j.NiceToHave,
                CompanyInforId = j.CompanyInforId,
                CompanyName = j.CompanyInfor.CompanyName,
                Location = j.Location,
                Status = j.Status,
                CategoryId = j.CategoryId,
                CategoryName = j.Category.Name,
                UpdatedBy = j.UpdatedBy,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt
            });

            return Ok(response);
        }

        // POST: api/Jobs
        [HttpPost]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<JobResponseDto>> CreateJob(JobDto jobDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get company info from user
            var companyInfor = await _unitOfWork.CompanyInfors.GetByUserIdAsync(userId);
            if (companyInfor == null)
            {
                return BadRequest(new { Message = "Bạn chưa tạo thông tin công ty. Vui lòng tạo thông tin công ty trước." });
            }

            // Verify category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(jobDto.CategoryId);
            if (category == null || category.DeletedAt != null)
            {
                return BadRequest(new { Message = "Danh mục không hợp lệ." });
            }

            var job = new Job
            {
                Title = jobDto.Title,
                Description = jobDto.Description,
                SalaryRange = jobDto.SalaryRange,
                DayOfWeek = jobDto.DayOfWeek,
                TimeOfDay = jobDto.TimeOfDay,
                Benefits = jobDto.Benefits,
                Requirements = jobDto.Requirements,
                NiceToHave = jobDto.NiceToHave,
                CompanyInforId = companyInfor.Id, // Tự động lấy từ user
                Location = jobDto.Location,
                Status = jobDto.Status,
                CategoryId = jobDto.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.Jobs.AddAsync(job);
            await _unitOfWork.SaveChangesAsync();

            var createdJob = await _unitOfWork.Jobs.GetJobWithDetailsAsync(job.Id);

            var response = new JobResponseDto
            {
                Id = createdJob.Id,
                Title = createdJob.Title,
                Description = createdJob.Description,
                SalaryRange = createdJob.SalaryRange,
                DayOfWeek = createdJob.DayOfWeek,
                TimeOfDay = createdJob.TimeOfDay,
                Benefits = createdJob.Benefits,
                Requirements = createdJob.Requirements,
                NiceToHave = createdJob.NiceToHave,
                CompanyInforId = createdJob.CompanyInforId,
                CompanyName = createdJob.CompanyInfor.CompanyName,
                Location = createdJob.Location,
                Status = createdJob.Status,
                CategoryId = createdJob.CategoryId,
                CategoryName = createdJob.Category.Name,
                UpdatedBy = createdJob.UpdatedBy,
                CreatedAt = createdJob.CreatedAt,
                UpdatedAt = createdJob.UpdatedAt
            };

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, response);
        }

        // PUT: api/Jobs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateJob(int id, JobDto jobDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(id);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            if (job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            // Verify category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(jobDto.CategoryId);
            if (category == null || category.DeletedAt != null)
            {
                return BadRequest(new { Message = "Danh mục không hợp lệ." });
            }

            job.Title = jobDto.Title;
            job.Description = jobDto.Description;
            job.SalaryRange = jobDto.SalaryRange;
            job.DayOfWeek = jobDto.DayOfWeek;
            job.TimeOfDay = jobDto.TimeOfDay;
            job.Benefits = jobDto.Benefits;
            job.Requirements = jobDto.Requirements;
            job.NiceToHave = jobDto.NiceToHave;
            job.Location = jobDto.Location;
            job.Status = jobDto.Status;
            job.CategoryId = jobDto.CategoryId;
            job.UpdatedAt = DateTime.UtcNow;
            job.UpdatedBy = userId;

            _unitOfWork.Jobs.Update(job);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(id);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            if (userRole != "Admin" && job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            job.DeletedAt = DateTime.UtcNow;
            job.UpdatedBy = userId;

            _unitOfWork.Jobs.Update(job);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
