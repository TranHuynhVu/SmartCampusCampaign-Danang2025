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

        #region Student APIs

        // POST: api/UserJobs/Apply - Sinh viên ứng tuyển vào công việc
        [HttpPost("Apply")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<UserJobResponseDto>> ApplyForJob([FromBody] UserJobDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(dto.JobId);
            if (job == null || job.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            // Kiểm tra đã ứng tuyển chưa
            var existing = await _unitOfWork.UserJobs.GetApplicationAsync(userId, dto.JobId);
            if (existing != null)
            {
                return BadRequest(new { Message = "Bạn đã ứng tuyển công việc này rồi." });
            }

            var userJob = new UserJob
            {
                UserId = userId,
                JobId = dto.JobId,
                Type = "Application",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.UserJobs.AddAsync(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = await MapToResponseDto(userJob, job);
            return Ok(response);
        }

        // GET: api/UserJobs/MyApplications - Sinh viên xem các đơn ứng tuyển của mình
        [HttpGet("MyApplications")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJobs = await _unitOfWork.UserJobs.GetApplicationsByUserIdAsync(userId);
            var applications = userJobs.Where(uj => uj.Type == "Application" && uj.DeletedAt == null);

            var response = new List<UserJobResponseDto>();
            foreach (var app in applications)
            {
                response.Add(await MapToResponseDto(app));
            }

            return Ok(response);
        }

        // GET: api/UserJobs/MyInvitations - Sinh viên xem các lời mời nhận được
        [HttpGet("MyInvitations")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetMyInvitations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJobs = await _unitOfWork.UserJobs.GetApplicationsByUserIdAsync(userId);
            var invitations = userJobs.Where(uj => uj.Type == "Invitation" && uj.DeletedAt == null);

            var response = new List<UserJobResponseDto>();
            foreach (var inv in invitations)
            {
                response.Add(await MapToResponseDto(inv));
            }

            return Ok(response);
        }

        // POST: api/UserJobs/RespondInvitation/update/{id} - Sinh viên chấp nhận/từ chối lời mời
        [HttpPost("RespondInvitation/update/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> RespondToInvitation(int id, [FromBody] UserJobResponseStatusDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy lời mời." });
            }

            if (userJob.UserId != userId)
            {
                return Forbid();
            }

            if (userJob.Type != "Invitation")
            {
                return BadRequest(new { Message = "Đây không phải là lời mời." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Lời mời này đã được phản hồi rồi." });
            }

            userJob.Status = dto.Status;
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = $"Lời mời đã được {(dto.Status == "Accepted" ? "chấp nhận" : "từ chối")}." });
        }

        // POST: api/UserJobs/AcceptInvitation/update/{id} - Sinh viên chấp nhận lời mời
        [HttpPost("AcceptInvitation/update/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> AcceptInvitation(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy lời mời." });
            }

            if (userJob.UserId != userId)
            {
                return Forbid();
            }

            if (userJob.Type != "Invitation")
            {
                return BadRequest(new { Message = "Đây không phải là lời mời." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Lời mời này đã được phản hồi rồi." });
            }

            userJob.Status = "Accepted";
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Lời mời đã được chấp nhận." });
        }

        // POST: api/UserJobs/RejectInvitation/update/{id} - Sinh viên từ chối lời mời
        [HttpPost("RejectInvitation/update/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> RejectInvitation(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy lời mời." });
            }

            if (userJob.UserId != userId)
            {
                return Forbid();
            }

            if (userJob.Type != "Invitation")
            {
                return BadRequest(new { Message = "Đây không phải là lời mời." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Lời mời này đã được phản hồi rồi." });
            }

            userJob.Status = "Rejected";
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Lời mời đã được từ chối." });
        }

        // POST: api/UserJobs/WithdrawApplication/delete/{id} - Sinh viên rút đơn ứng tuyển
        [HttpPost("WithdrawApplication/delete/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult> WithdrawApplication(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userJob.UserId != userId)
            {
                return Forbid();
            }

            if (userJob.Type != "Application")
            {
                return BadRequest(new { Message = "Đây không phải là đơn ứng tuyển." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Không thể rút đơn ứng tuyển đã được phản hồi." });
            }

            userJob.DeletedAt = DateTime.Now;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Đơn ứng tuyển đã được rút thành công." });
        }

        #endregion

        #region Company APIs

        // POST: api/UserJobs/SendInvitation - Doanh nghiệp gửi lời mời cho ứng viên
        [HttpPost("SendInvitation")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<UserJobResponseDto>> SendInvitation([FromBody] CompanyInvitationDto dto)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(dto.JobId);
            if (job == null || job.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            if (job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            // Kiểm tra student có tồn tại
            var student = await _unitOfWork.StudentInfors.GetByUserIdAsync(dto.StudentUserId);
            if (student == null || student.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy ứng viên." });
            }

            // Kiểm tra đã gửi lời mời chưa
            var existing = await _unitOfWork.UserJobs.GetApplicationAsync(dto.StudentUserId, dto.JobId);
            if (existing != null)
            {
                return BadRequest(new { Message = "Đã có quan hệ tuyển dụng với ứng viên này cho công việc này." });
            }

            var userJob = new UserJob
            {
                UserId = dto.StudentUserId,
                JobId = dto.JobId,
                Type = "Invitation",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = companyUserId
            };

            await _unitOfWork.UserJobs.AddAsync(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = await MapToResponseDto(userJob, job);
            return Ok(response);
        }

        // GET: api/UserJobs/SentInvitations - Doanh nghiệp xem các lời mời đã gửi
        [HttpGet("SentInvitations")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetSentInvitations()
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var companyInfor = await _unitOfWork.CompanyInfors.GetByUserIdAsync(companyUserId);
            if (companyInfor == null)
            {
                return BadRequest(new { Message = "Bạn chưa tạo thông tin công ty." });
            }

            var jobs = await _unitOfWork.Jobs.GetJobsByUserIdAsync(companyUserId);
            var jobIds = jobs.Select(j => j.Id).ToList();

            var allUserJobs = await _unitOfWork.UserJobs.GetAllAsync();
            var sentInvitations = allUserJobs
                .Where(uj => jobIds.Contains(uj.JobId) && uj.Type == "Invitation" && uj.DeletedAt == null)
                .ToList();

            var response = new List<UserJobResponseDto>();
            foreach (var inv in sentInvitations)
            {
                response.Add(await MapToResponseDto(inv));
            }

            return Ok(response);
        }

        // GET: api/UserJobs/ReceivedApplications/{jobId} - Doanh nghiệp xem đơn ứng tuyển cho 1 công việc
        [HttpGet("ReceivedApplications/{jobId}")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetReceivedApplications(int jobId)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            if (job == null || job.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            if (job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            var applications = await _unitOfWork.UserJobs.GetApplicationsByJobIdAsync(jobId);
            var filtered = applications.Where(uj => uj.Type == "Application" && uj.DeletedAt == null);

            var response = new List<UserJobResponseDto>();
            foreach (var app in filtered)
            {
                response.Add(await MapToResponseDto(app));
            }

            return Ok(response);
        }

        // GET: api/UserJobs/AllReceivedApplications - Doanh nghiệp xem tất cả đơn ứng tuyển
        [HttpGet("AllReceivedApplications")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetAllReceivedApplications()
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var companyInfor = await _unitOfWork.CompanyInfors.GetByUserIdAsync(companyUserId);
            if (companyInfor == null)
            {
                return BadRequest(new { Message = "Bạn chưa tạo thông tin công ty." });
            }

            var jobs = await _unitOfWork.Jobs.GetJobsByUserIdAsync(companyUserId);
            var jobIds = jobs.Select(j => j.Id).ToList();

            var allUserJobs = await _unitOfWork.UserJobs.GetAllAsync();
            var receivedApplications = allUserJobs
                .Where(uj => jobIds.Contains(uj.JobId) && uj.Type == "Application" && uj.DeletedAt == null)
                .ToList();

            var response = new List<UserJobResponseDto>();
            foreach (var app in receivedApplications)
            {
                response.Add(await MapToResponseDto(app));
            }

            return Ok(response);
        }

        // POST: api/UserJobs/RespondApplication/update/{id} - Doanh nghiệp chấp nhận/từ chối đơn ứng tuyển
        [HttpPost("RespondApplication/update/{id}")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult> RespondToApplication(int id, [FromBody] UserJobResponseStatusDto dto)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userJob.Job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            if (userJob.Type != "Application")
            {
                return BadRequest(new { Message = "Đây không phải là đơn ứng tuyển." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Đơn ứng tuyển này đã được phản hồi rồi." });
            }

            userJob.Status = dto.Status;
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = companyUserId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = $"Đơn ứng tuyển đã được {(dto.Status == "Accepted" ? "chấp nhận" : "từ chối")}." });
        }

        // POST: api/UserJobs/AcceptApplication/update/{id} - Doanh nghiệp chấp nhận đơn ứng tuyển
        [HttpPost("AcceptApplication/update/{id}")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult> AcceptApplication(int id)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userJob.Job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            if (userJob.Type != "Application")
            {
                return BadRequest(new { Message = "Đây không phải là đơn ứng tuyển." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Đơn ứng tuyển này đã được phản hồi rồi." });
            }

            userJob.Status = "Accepted";
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = companyUserId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Đơn ứng tuyển đã được chấp nhận." });
        }

        // POST: api/UserJobs/RejectApplication/update/{id} - Doanh nghiệp từ chối đơn ứng tuyển
        [HttpPost("RejectApplication/update/{id}")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult> RejectApplication(int id)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            if (userJob.Job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            if (userJob.Type != "Application")
            {
                return BadRequest(new { Message = "Đây không phải là đơn ứng tuyển." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Đơn ứng tuyển này đã được phản hồi rồi." });
            }

            userJob.Status = "Rejected";
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = companyUserId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Đơn ứng tuyển đã được từ chối." });
        }

        // POST: api/UserJobs/WithdrawInvitation/delete/{id} - Doanh nghiệp rút lời mời
        [HttpPost("WithdrawInvitation/delete/{id}")]
        [Authorize(Roles = "Company")]
        public async Task<ActionResult> WithdrawInvitation(int id)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy lời mời." });
            }

            if (userJob.Job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            if (userJob.Type != "Invitation")
            {
                return BadRequest(new { Message = "Đây không phải là lời mời." });
            }

            if (userJob.Status != "Pending")
            {
                return BadRequest(new { Message = "Không thể rút lời mời đã được phản hồi." });
            }

            userJob.DeletedAt = DateTime.Now;
            userJob.UpdatedBy = companyUserId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "Lời mời đã được rút thành công." });
        }

        #endregion

        #region Common APIs

        // GET: api/UserJobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserJobResponseDto>> GetUserJob(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy." });
            }

            // Chỉ cho phép student sở hữu, company sở hữu job, hoặc Admin
            bool canView = userRole == "Admin" ||
                          userJob.UserId == userId ||
                          (userRole == "Company" && userJob.Job.CompanyInfor.UserId == userId);

            if (!canView)
            {
                return Forbid();
            }

            var response = await MapToResponseDto(userJob);
            return Ok(response);
        }

        // POST: api/UserJobs/delete/{id}
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteUserJob(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);
            if (userJob == null || userJob.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy." });
            }

            // Chỉ cho phép xóa nếu là Admin hoặc người tạo và đang Pending
            bool canDelete = userRole == "Admin" ||
                            (userJob.Status == "Pending" && 
                             ((userJob.Type == "Application" && userJob.UserId == userId) ||
                              (userJob.Type == "Invitation" && userJob.Job.CompanyInfor.UserId == userId)));

            if (!canDelete)
            {
                return Forbid();
            }

            userJob.DeletedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Helper Methods

        private async Task<UserJobResponseDto> MapToResponseDto(UserJob userJob, Job? job = null)
        {
            var jobData = job ?? userJob.Job;

            // Lấy thông tin student
            string? studentName = null;
            string? studentAvatar = null;
            string? studentEmail = null;
            string? studentGPA = null;
            string? studentSkills = null;
            string? studentYearOfStudy = null;
            string? studentMajor = null;
            string? studentResumeUrl = null;
            string? studentLanguages = null;
            string? studentExperiences = null;
            bool? studentOpenToWork = null;

            if (userJob.UserId != null)
            {
                var student = await _unitOfWork.StudentInfors.GetByUserIdAsync(userJob.UserId);
                if (student != null)
                {
                    studentName = student.Name;
                    studentAvatar = student.AvatarUrl;
                    studentGPA = student.GPA;
                    studentSkills = student.Skills;
                    studentYearOfStudy = student.YearOfStudy;
                    studentMajor = student.Major;
                    studentResumeUrl = student.ResumeUrl;
                    studentLanguages = student.Languages;
                    studentExperiences = student.Experiences;
                    studentOpenToWork = student.OpenToWork;
                }
                
                // Lấy email từ User
                if (userJob.User != null)
                {
                    studentEmail = userJob.User.Email;
                }
            }

            return new UserJobResponseDto
            {
                Id = userJob.Id,
                UserId = userJob.UserId,
                
                // Student Information
                StudentName = studentName,
                StudentAvatar = studentAvatar,
                StudentEmail = studentEmail,
                StudentGPA = studentGPA,
                StudentSkills = studentSkills,
                StudentYearOfStudy = studentYearOfStudy,
                StudentMajor = studentMajor,
                StudentResumeUrl = studentResumeUrl,
                StudentLanguages = studentLanguages,
                StudentExperiences = studentExperiences,
                StudentOpenToWork = studentOpenToWork,
                
                // Job Information
                JobId = userJob.JobId,
                JobTitle = jobData?.Title,
                JobDescription = jobData?.Description,
                JobSalaryRange = jobData?.SalaryRange,
                JobLocation = jobData?.Location,
                JobDayOfWeek = jobData?.DayOfWeek,
                JobTimeOfDay = jobData?.TimeOfDay,
                JobBenefits = jobData?.Benefits,
                JobRequirements = jobData?.Requirements,
                JobStatus = jobData?.Status,
                
                // Company Information
                CompanyInforId = jobData?.CompanyInforId,
                CompanyName = jobData?.CompanyInfor?.CompanyName,
                CompanyLogoUrl = jobData?.CompanyInfor?.LogoUrl,
                CompanyWebsite = jobData?.CompanyInfor?.CompanyWebsite,
                CompanyLocation = jobData?.CompanyInfor?.Location,
                CompanyDescriptions = jobData?.CompanyInfor?.Descriptions,
                
                // Category Information
                CategoryId = jobData?.CategoryId,
                CategoryName = jobData?.Category?.Name,
                
                // UserJob Information
                Type = userJob.Type,
                Status = userJob.Status,
                UpdatedBy = userJob.UpdatedBy,
                CreatedAt = userJob.CreatedAt,
                UpdatedAt = userJob.UpdatedAt
            };
        }

        #endregion
    }
}
