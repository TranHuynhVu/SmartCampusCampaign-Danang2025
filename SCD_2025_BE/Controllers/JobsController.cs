using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;
using SCD_2025_BE.Service;
using System.Security.Claims;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGemini _geminiService;
        private readonly UserManager<AppUser> _userManager;

        public JobsController(IUnitOfWork unitOfWork, IGemini geminiService, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _geminiService = geminiService;
            _userManager = userManager;
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

            // Tạo embedding từ Requirements và NiceToHave
            job.Embeddings = await _geminiService.GetJobEmbedding(job);

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

        // POST: api/Jobs/update/5
        [HttpPost("update/{id}")]
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

            // Cập nhật lại embedding nếu Requirements hoặc NiceToHave thay đổi
            job.Embeddings = await _geminiService.GetJobEmbedding(job);

            _unitOfWork.Jobs.Update(job);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Jobs/delete/5
        [HttpPost("delete/{id}")]
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

        // GET: api/Jobs/CandidateSuggestions/{jobId}
        [HttpGet("CandidateSuggestions/{jobId}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<ActionResult<IEnumerable<CandidateSuggestionDto>>> GetCandidateSuggestions(int jobId, [FromQuery] int top = 10)
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            
            if (job == null || job.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            // Kiểm tra quyền truy cập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(job.Embeddings))
            {
                return BadRequest(new { Message = "Công việc chưa có embedding. Vui lòng cập nhật công việc." });
            }

            var jobEmbedding = JsonSerializer.Deserialize<List<double>>(job.Embeddings);
            if (jobEmbedding == null || jobEmbedding.Count == 0)
            {
                return BadRequest(new { Message = "Embedding không hợp lệ." });
            }

            // Lấy tất cả students đang active VÀ OpenToWork = true
            var allStudents = await _unitOfWork.StudentInfors.GetActiveStudentsAsync();
            var students = allStudents.Where(s => s.OpenToWork == true).ToList();

            var candidateSuggestions = new List<CandidateSuggestionDto>();

            foreach (var student in students)
            {
                if (string.IsNullOrWhiteSpace(student.EmBeddings))
                    continue;

                var studentEmbedding = JsonSerializer.Deserialize<List<double>>(student.EmBeddings);
                if (studentEmbedding == null || studentEmbedding.Count == 0)
                    continue;

                var similarity = _geminiService.CosineSimilarity(jobEmbedding, studentEmbedding);

                candidateSuggestions.Add(new CandidateSuggestionDto
                {
                    Id = student.Id,
                    UserId = student.UserId,
                    Name = student.Name,
                    ResumeUrl = student.ResumeUrl,
                    GPA = student.GPA,
                    Skills = student.Skills,
                    Archievements = student.Archievements,
                    YearOfStudy = student.YearOfStudy,
                    Major = student.Major,
                    Languages = student.Languages,
                    Certifications = student.Certifications,
                    Experiences = student.Experiences,
                    Projects = student.Projects,
                    Educations = student.Educations,
                    AvatarUrl = student.AvatarUrl,
                    OpenToWork = student.OpenToWork,
                    UpdatedBy = student.UpdatedBy,
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt,
                    SimilarityScore = similarity
                });
            }

            // Sắp xếp theo độ tương đồng giảm dần và lấy top kết quả
            var topSuggestions = candidateSuggestions
                .OrderByDescending(c => c.SimilarityScore)
                .Take(top)
                .ToList();

            return Ok(topSuggestions);
        }

        // GET: api/Jobs/ExportCandidateSuggestions/{jobId}
        [HttpGet("ExportCandidateSuggestions/{jobId}")]
        [Authorize(Roles = "Company,Admin")]
        public async Task<IActionResult> ExportCandidateSuggestions(int jobId, [FromQuery] int top = 10)
        {
            var job = await _unitOfWork.Jobs.GetJobWithDetailsAsync(jobId);
            
            if (job == null || job.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy công việc." });
            }

            // Kiểm tra quyền truy cập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && job.CompanyInfor.UserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(job.Embeddings))
            {
                return BadRequest(new { Message = "Công việc chưa có embedding. Vui lòng cập nhật công việc." });
            }

            var jobEmbedding = JsonSerializer.Deserialize<List<double>>(job.Embeddings);
            if (jobEmbedding == null || jobEmbedding.Count == 0)
            {
                return BadRequest(new { Message = "Embedding không hợp lệ." });
            }

            // Lấy tất cả students đang active VÀ OpenToWork = true
            var allStudents = await _unitOfWork.StudentInfors.GetActiveStudentsAsync();
            var students = allStudents.Where(s => s.OpenToWork == true).ToList();

            var candidateSuggestions = new List<CandidateSuggestionDto>();

            foreach (var student in students)
            {
                if (string.IsNullOrWhiteSpace(student.EmBeddings))
                    continue;

                var studentEmbedding = JsonSerializer.Deserialize<List<double>>(student.EmBeddings);
                if (studentEmbedding == null || studentEmbedding.Count == 0)
                    continue;

                var similarity = _geminiService.CosineSimilarity(jobEmbedding, studentEmbedding);

                candidateSuggestions.Add(new CandidateSuggestionDto
                {
                    Id = student.Id,
                    UserId = student.UserId,
                    Name = student.Name,
                    ResumeUrl = student.ResumeUrl,
                    GPA = student.GPA,
                    Skills = student.Skills,
                    Archievements = student.Archievements,
                    YearOfStudy = student.YearOfStudy,
                    Major = student.Major,
                    Languages = student.Languages,
                    Certifications = student.Certifications,
                    Experiences = student.Experiences,
                    Projects = student.Projects,
                    Educations = student.Educations,
                    AvatarUrl = student.AvatarUrl,
                    OpenToWork = student.OpenToWork,
                    UpdatedBy = student.UpdatedBy,
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt,
                    SimilarityScore = similarity
                });
            }

            // Sắp xếp theo độ tương đồng giảm dần và lấy top kết quả
            var topSuggestions = candidateSuggestions
                .OrderByDescending(c => c.SimilarityScore)
                .Take(top)
                .ToList();

            // Tạo Excel file
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Candidate Suggestions");

            // Header
            worksheet.Cell(1, 1).Value = "STT";
            worksheet.Cell(1, 2).Value = "Tên sinh viên";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Số điện thoại";
            worksheet.Cell(1, 5).Value = "GPA";
            worksheet.Cell(1, 6).Value = "Năm học";
            worksheet.Cell(1, 7).Value = "Chuyên ngành";
            worksheet.Cell(1, 8).Value = "Kỹ năng";
            worksheet.Cell(1, 9).Value = "Ngôn ngữ";
            worksheet.Cell(1, 10).Value = "Kinh nghiệm";
            worksheet.Cell(1, 11).Value = "Dự án";
            worksheet.Cell(1, 12).Value = "Thành tích";
            worksheet.Cell(1, 13).Value = "Chứng chỉ";
            worksheet.Cell(1, 14).Value = "Học vấn";
            worksheet.Cell(1, 15).Value = "Độ phù hợp (%)";
            worksheet.Cell(1, 16).Value = "Link CV";

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 16);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data
            int row = 2;
            int stt = 1;
            foreach (var candidate in topSuggestions)
            {
                // Lấy email và phone từ User
                var user = await _userManager.FindByIdAsync(candidate.UserId);
                string email = user?.Email ?? "";
                string phone = user?.PhoneNumber ?? "";

                worksheet.Cell(row, 1).Value = stt;
                worksheet.Cell(row, 2).Value = candidate.Name ?? "";
                worksheet.Cell(row, 3).Value = email;
                worksheet.Cell(row, 4).Value = phone;
                worksheet.Cell(row, 5).Value = candidate.GPA ?? "";
                worksheet.Cell(row, 6).Value = candidate.YearOfStudy ?? "";
                worksheet.Cell(row, 7).Value = candidate.Major ?? "";
                worksheet.Cell(row, 8).Value = candidate.Skills ?? "";
                worksheet.Cell(row, 9).Value = candidate.Languages ?? "";
                worksheet.Cell(row, 10).Value = candidate.Experiences ?? "";
                worksheet.Cell(row, 11).Value = candidate.Projects ?? "";
                worksheet.Cell(row, 12).Value = candidate.Archievements ?? "";
                worksheet.Cell(row, 13).Value = candidate.Certifications ?? "";
                worksheet.Cell(row, 14).Value = candidate.Educations ?? "";
                worksheet.Cell(row, 15).Value = Math.Round(candidate.SimilarityScore * 100, 2);
                worksheet.Cell(row, 16).Value = candidate.ResumeUrl ?? "";

                row++;
                stt++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Tạo memory stream để trả về file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"CandidateSuggestions_{job.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
