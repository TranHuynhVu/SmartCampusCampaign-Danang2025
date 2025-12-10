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
    public class UserJobsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserJobsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetUserJobs()
        {
            var userJobs = await _unitOfWork.UserJobs.GetAllAsync();

            var response = userJobs
                .Where(uj => uj.DeletedAt == null)
                .Select(uj => new UserJobResponseDto
                {
                    Id = uj.Id,
                    UserId = uj.UserId,
                    JobId = uj.JobId,
                    JobTitle = uj.Job?.Title,
                    CompanyName = uj.Job?.CompanyInfor?.CompanyName,
                    Status = uj.Status,
                    UpdatedBy = uj.UpdatedBy,
                    CreatedAt = uj.CreatedAt,
                    UpdatedAt = uj.UpdatedAt
                });

            return Ok(response);
        }

        [HttpGet("MyApplications")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJobs = await _unitOfWork.UserJobs.GetApplicationsByUserIdAsync(userId);

            var response = userJobs.Select(uj => new UserJobResponseDto
            {
                Id = uj.Id,
                UserId = uj.UserId,
                JobId = uj.JobId,
                JobTitle = uj.Job.Title,
                CompanyName = uj.Job.CompanyInfor.CompanyName,
                Status = uj.Status,
                UpdatedBy = uj.UpdatedBy,
                CreatedAt = uj.CreatedAt,
                UpdatedAt = uj.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("JobApplications/{jobId}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetJobApplications(int jobId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            if (userRole != "Admin" && job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            var applications = await _unitOfWork.UserJobs.GetApplicationsByJobIdAsync(jobId);

            var response = applications.Select(uj => new UserJobResponseDto
            {
                Id = uj.Id,
                UserId = uj.UserId,
                JobId = uj.JobId,
                JobTitle = uj.Job.Title,
                CompanyName = uj.Job.CompanyInfor.CompanyName,
                Status = uj.Status,
                UpdatedBy = uj.UpdatedBy,
                CreatedAt = uj.CreatedAt,
                UpdatedAt = uj.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserJobResponseDto>> GetUserJob(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);

            if (userJob == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userRole != "Admin" && userJob.UserId != userId && userJob.Job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            var response = new UserJobResponseDto
            {
                Id = userJob.Id,
                UserId = userJob.UserId,
                JobId = userJob.JobId,
                JobTitle = userJob.Job.Title,
                CompanyName = userJob.Job.CompanyInfor.CompanyName,
                Status = userJob.Status,
                UpdatedBy = userJob.UpdatedBy,
                CreatedAt = userJob.CreatedAt,
                UpdatedAt = userJob.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<UserJobResponseDto>> ApplyForJob(UserJobDto userJobDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(userJobDto.JobId);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            var existingApplication = await _unitOfWork.UserJobs.GetApplicationAsync(userId, userJobDto.JobId);

            if (existingApplication != null)
            {
                return BadRequest(new { Message = "Bạn đã ứng tuyển công việc này rồi." });
            }

            var userJob = new UserJob
            {
                UserId = userId,
                JobId = userJobDto.JobId,
                Status = userJobDto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.UserJobs.AddAsync(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = new UserJobResponseDto
            {
                Id = userJob.Id,
                UserId = userJob.UserId,
                JobId = userJob.JobId,
                JobTitle = job.Title,
                CompanyName = job.CompanyInfor.CompanyName,
                Status = userJob.Status,
                UpdatedBy = userJob.UpdatedBy,
                CreatedAt = userJob.CreatedAt,
                UpdatedAt = userJob.UpdatedAt
            };

            return CreatedAtAction(nameof(GetUserJob), new { id = userJob.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserJob(int id, UserJobDto userJobDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);

            if (userJob == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userRole == "Student" && userJob.UserId != userId)
            {
                return Forbid();
            }

            if (userRole == "Company" && userJob.Job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            userJob.Status = userJobDto.Status;
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserJob(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);

            if (userJob == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userRole != "Admin" && userJob.UserId != userId)
            {
                return Forbid();
            }

            userJob.DeletedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
