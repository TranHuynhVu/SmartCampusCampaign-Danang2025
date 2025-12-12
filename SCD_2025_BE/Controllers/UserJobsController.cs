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

        // GET: api/UserJobs (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetUserJobs()
        {
            var userJobs = await _unitOfWork.UserJobs.GetAllAsync();

            var response = userJobs
                .Where(uj => uj.DeletedAt == null)
                .Select(uj => MapToResponseDto(uj));

            return Ok(response);
        }

        // GET: api/UserJobs/MyApplications (Student only)
        [HttpGet("MyApplications")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJobs = await _unitOfWork.UserJobs.GetApplicationsByUserIdAsync(userId);

            // Filter: Chỉ lấy những đơn mà Student là người tạo
            var myApplications = userJobs
                .Where(uj => IsStudentInitiated(uj))
                .Select(uj => MapToResponseDto(uj));

            return Ok(myApplications);
        }

        // GET: api/UserJobs/MyInvitations (Student only)
        [HttpGet("MyInvitations")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IEnumerable<UserJobResponseDto>>> GetMyInvitations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userJobs = await _unitOfWork.UserJobs.GetApplicationsByUserIdAsync(userId);

            // Filter: Chỉ lấy những lời mời từ Company
            var myInvitations = userJobs
                .Where(uj => IsCompanyInitiated(uj))
                .Select(uj => MapToResponseDto(uj));

            return Ok(myInvitations);
        }

        // GET: api/UserJobs/JobApplications/{jobId} (Company + Admin)
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

            var response = applications.Select(uj => MapToResponseDto(uj));

            return Ok(response);
        }

        // GET: api/UserJobs/{id}
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

            var response = MapToResponseDto(userJob);

            return Ok(response);
        }

        // POST: api/UserJobs (Student Apply)
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
                Status = "Applied", // Student apply luôn bắt đầu với Applied
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.UserJobs.AddAsync(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = MapToResponseDto(userJob, job);

            return CreatedAtAction(nameof(GetUserJob), new { id = userJob.Id }, response);
        }

        // POST: api/UserJobs/InviteCandidate (Company Recruit)
        [HttpPost("InviteCandidate")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<ActionResult<UserJobResponseDto>> InviteCandidate([FromBody] CompanyRecruitDto recruitDto)
        {
            var companyUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(recruitDto.JobId);

            if (job == null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            // Verify company owns this job
            if (userRole != "Admin" && job.CompanyInfor.UserId != companyUserId)
            {
                return Forbid();
            }

            // Check if student exists
            var student = await _unitOfWork.StudentInfors.GetByUserIdAsync(recruitDto.UserId);
            if (student == null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin sinh viên." });
            }

            var existingRelation = await _unitOfWork.UserJobs.GetApplicationAsync(recruitDto.UserId, recruitDto.JobId);

            if (existingRelation != null)
            {
                return BadRequest(new { Message = "Đã có quan hệ tuyển dụng với ứng viên này cho công việc này." });
            }

            var userJob = new UserJob
            {
                UserId = recruitDto.UserId,
                JobId = recruitDto.JobId,
                Status = "Applied", // Company invite cũng bắt đầu với Applied
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = companyUserId // Company là người tạo
            };

            await _unitOfWork.UserJobs.AddAsync(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = MapToResponseDto(userJob, job);

            return CreatedAtAction(nameof(GetUserJob), new { id = userJob.Id }, response);
        }

        // PUT: api/UserJobs/{id} (Update Status)
        [HttpPut("{id}")]
        public async Task<ActionResult<UserJobResponseDto>> UpdateUserJob(int id, [FromBody] UserJobUpdateDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userJob = await _unitOfWork.UserJobs.GetApplicationWithDetailsAsync(id);

            if (userJob == null)
            {
                return NotFound(new { Message = "Không tìm thấy đơn ứng tuyển." });
            }

            // Check authorization
            bool isStudent = userRole == "Student" && userJob.UserId == userId;
            bool isCompany = userRole == "Company" && userJob.Job.CompanyInfor.UserId == userId;
            bool isAdmin = userRole == "Admin";

            if (!isStudent && !isCompany && !isAdmin)
            {
                return Forbid();
            }

            // Validate status transition
            var validationResult = ValidateStatusTransition(userJob, updateDto.Status, userId, userRole);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = validationResult.ErrorMessage });
            }

            userJob.Status = updateDto.Status;
            userJob.UpdatedAt = DateTime.UtcNow;
            userJob.UpdatedBy = userId;

            _unitOfWork.UserJobs.Update(userJob);
            await _unitOfWork.SaveChangesAsync();

            var response = MapToResponseDto(userJob);

            return Ok(response);
        }

        // DELETE: api/UserJobs/{id} (Soft Delete)
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

            // Only Admin or the student who created the application can delete
            bool canDelete = userRole == "Admin" || 
                           (userRole == "Student" && userJob.UserId == userId && IsStudentInitiated(userJob));

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

        #region Helper Methods

        private UserJobResponseDto MapToResponseDto(UserJob userJob, Job? job = null)
        {
            var jobData = job ?? userJob.Job;

            return new UserJobResponseDto
            {
                Id = userJob.Id,
                UserId = userJob.UserId,
                JobId = userJob.JobId,
                JobTitle = jobData?.Title,
                CompanyName = jobData?.CompanyInfor?.CompanyName,
                Status = userJob.Status,
                UpdatedBy = userJob.UpdatedBy,
                CreatedAt = userJob.CreatedAt,
                UpdatedAt = userJob.UpdatedAt
            };
        }

        private bool IsStudentInitiated(UserJob userJob)
        {
            // Student initiated if UserId == UpdatedBy at creation time
            bool isNewRecord = userJob.CreatedAt == userJob.UpdatedAt;
            return isNewRecord && userJob.UserId == userJob.UpdatedBy;
        }

        private bool IsCompanyInitiated(UserJob userJob)
        {
            // Company initiated if UserId != UpdatedBy at creation time
            bool isNewRecord = userJob.CreatedAt == userJob.UpdatedAt;
            return isNewRecord && userJob.UserId != userJob.UpdatedBy;
        }

        private (bool IsValid, string ErrorMessage) ValidateStatusTransition(
            UserJob userJob, 
            string newStatus, 
            string userId, 
            string userRole)
        {
            var currentStatus = userJob.Status;

            // Rule 1: Cannot change final states
            if (currentStatus == "Accepted" || currentStatus == "Rejected" || currentStatus == "Withdrawn")
            {
                return (false, $"Không thể thay đổi trạng thái '{currentStatus}'. Đây là trạng thái kết thúc.");
            }

            // Rule 2: Validate status flow
            if (currentStatus == "Applied" && newStatus != "Reviewing" && 
                newStatus != "Accepted" && newStatus != "Rejected" && newStatus != "Withdrawn")
            {
                return (false, $"Không thể chuyển từ '{currentStatus}' sang '{newStatus}'.");
            }

            if (currentStatus == "Reviewing" && newStatus != "Accepted" && 
                newStatus != "Rejected" && newStatus != "Withdrawn")
            {
                return (false, $"Không thể chuyển từ '{currentStatus}' sang '{newStatus}'.");
            }

            // Rule 3: Withdrawn only by creator
            if (newStatus == "Withdrawn")
            {
                bool isStudentCreator = IsStudentInitiated(userJob);
                bool isCompanyCreator = IsCompanyInitiated(userJob);

                if (isStudentCreator && userRole != "Student")
                {
                    return (false, "Chỉ sinh viên tạo đơn mới có thể rút đơn.");
                }

                if (isCompanyCreator && userRole != "Company" && userRole != "Admin")
                {
                    return (false, "Chỉ công ty tạo lời mời mới có thể rút lời mời.");
                }
            }

            // Rule 4: Role-based status change
            bool isStudentInitiated = IsStudentInitiated(userJob);

            if (isStudentInitiated && userRole == "Student" && 
                (newStatus == "Accepted" || newStatus == "Rejected"))
            {
                return (false, "Sinh viên không thể tự chấp nhận/từ chối đơn của mình. Chỉ công ty mới có quyền này.");
            }

            if (!isStudentInitiated && userRole == "Company" && 
                (newStatus == "Accepted" || newStatus == "Rejected"))
            {
                return (false, "Công ty không thể tự chấp nhận/từ chối lời mời của mình. Chỉ sinh viên mới có quyền này.");
            }

            return (true, string.Empty);
        }

        #endregion
    }
}
