using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;
using SCD_2025_BE.Service;
using System.Security.Claims;
using System.Text.Json;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentInforsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGemini _geminiService;

        public StudentInforsController(IUnitOfWork unitOfWork, IGemini geminiService)
        {
            _unitOfWork = unitOfWork;
            _geminiService = geminiService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Company")]
        public async Task<ActionResult<IEnumerable<StudentInforResponseDto>>> GetStudentInfors()
        {
            var students = await _unitOfWork.StudentInfors.GetActiveStudentsAsync();

            var response = students.Select(s => new StudentInforResponseDto
            {
                Id = s.Id,
                UserId = s.UserId,
                Name = s.Name,
                ResumeUrl = s.ResumeUrl,
                GPA = s.GPA,
                Skills = s.Skills,
                Archievements = s.Archievements,
                YearOfStudy = s.YearOfStudy,
                Major = s.Major,
                Languages = s.Languages,
                Certifications = s.Certifications,
                Experiences = s.Experiences,
                Projects = s.Projects,
                Educations = s.Educations,
                AvatarUrl = s.AvatarUrl,
                OpenToWork = s.OpenToWork,
                UpdatedBy = s.UpdatedBy,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<ActionResult<StudentInforResponseDto>> GetStudentInfor(int id)
        {
            var student = await _unitOfWork.StudentInfors.GetByIdAsync(id);

            if (student == null || student.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin sinh viên." });
            }

            var response = new StudentInforResponseDto
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
                UpdatedAt = student.UpdatedAt
            };

            return Ok(response);
        }

        [HttpGet("MyProfile")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentInforResponseDto>> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var student = await _unitOfWork.StudentInfors.GetByUserIdAsync(userId);

            if (student == null)
            {
                return NotFound(new { Message = "Bạn chưa tạo hồ sơ sinh viên." });
            }

            var response = new StudentInforResponseDto
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
                UpdatedAt = student.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<StudentInforResponseDto>> CreateStudentInfor([FromForm] StudentInforDto studentDto, IFormFile? resumeFile = null, IFormFile? avatarFile = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingStudent = await _unitOfWork.StudentInfors.GetByUserIdAsync(userId);

            // Nếu đã có hồ sơ, chuyển sang update
            if (existingStudent != null)
            {
                return await UpdateExistingStudent(existingStudent, studentDto, resumeFile, avatarFile, userId);
            }

            // Xử lý file upload
            string? resumeUrl = null;
            string? avatarUrl = null;
            byte[]? pdfBytes = null;
            
            if (resumeFile != null)
            {
                // Kiểm tra file có phải là PDF không
                if (resumeFile.ContentType != "application/pdf")
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file PDF cho CV." });
                }

                // Kiểm tra kích thước file (giới hạn 5MB)
                if (resumeFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File CV không được vượt quá 5MB." });
                }

                // Đọc file thành bytes để gửi cho AI
                using (var memoryStream = new MemoryStream())
                {
                    await resumeFile.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(resumeFile.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                resumeUrl = $"/resumes/{fileName}";
            }

            // Xử lý avatar upload
            if (avatarFile != null)
            {
                // Kiểm tra file có phải là hình ảnh không
                var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedImageTypes.Contains(avatarFile.ContentType.ToLower()))
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF) cho avatar." });
                }

                // Kiểm tra kích thước file (giới hạn 2MB)
                if (avatarFile.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File avatar không được vượt quá 2MB." });
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                if (!Directory.Exists(avatarFolder))
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                // Tạo tên file duy nhất
                var avatarFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var avatarFilePath = Path.Combine(avatarFolder, avatarFileName);

                // Lưu file
                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                avatarUrl = $"/avatars/{avatarFileName}";
            }

            StudentInfor student;

            // Nếu có file PDF, dùng AI phân tích CV
            if (pdfBytes != null)
            {
                // Phân tích CV bằng AI
                student = await _geminiService.ClassifyStudentCV(pdfBytes);
                
                if (student == null)
                {
                    return BadRequest(new { Message = "Không thể phân tích CV. Vui lòng thử lại." });
                }

                // Gán thông tin bổ sung
                student.UserId = userId;
                student.ResumeUrl = resumeUrl;
                student.AvatarUrl = avatarUrl;
                student.OpenToWork = studentDto.OpenToWork ?? true;
                student.CreatedAt = DateTime.UtcNow;
                student.UpdatedBy = userId;
                
                // Khi có PDF, sử dụng hoàn toàn dữ liệu từ AI, không ghi đè từ DTO
            }
            else
            {
                // Không có file PDF, tạo từ form data
                student = new StudentInfor
                {
                    UserId = userId,
                    Name = studentDto.Name,
                    ResumeUrl = resumeUrl,
                    AvatarUrl = avatarUrl,
                    GPA = studentDto.GPA,
                    Skills = studentDto.Skills,
                    Archievements = studentDto.Archievements,
                    YearOfStudy = studentDto.YearOfStudy,
                    Major = studentDto.Major,
                    Languages = studentDto.Languages,
                    Certifications = studentDto.Certifications,
                    Experiences = studentDto.Experiences,
                    Projects = studentDto.Projects,
                    Educations = studentDto.Educations,
                    OpenToWork = studentDto.OpenToWork ?? true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = userId
                };
            }

            // Tạo embedding cho student
            student.EmBeddings = await _geminiService.GetStudentEmbedding(student);

            await _unitOfWork.StudentInfors.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            var response = new StudentInforResponseDto
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
                UpdatedAt = student.UpdatedAt
            };

            return CreatedAtAction(nameof(GetStudentInfor), new { id = student.Id }, response);
        }

        private async Task<ActionResult<StudentInforResponseDto>> UpdateExistingStudent(StudentInfor student, StudentInforDto studentDto, IFormFile? resumeFile, IFormFile? avatarFile, string userId)
        {
            byte[]? pdfBytes = null;

            // Xử lý avatar upload nếu có
            if (avatarFile != null)
            {
                // Kiểm tra file có phải là hình ảnh không
                var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedImageTypes.Contains(avatarFile.ContentType.ToLower()))
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file ảnh (JPEG, PNG, GIF) cho avatar." });
                }

                // Kiểm tra kích thước file (giới hạn 4MB)
                if (avatarFile.Length > 2 * 2024 * 2024)
                {
                    return BadRequest(new { Message = "File avatar không được vượt quá 2MB." });
                }

                // Xóa file cũ nếu tồn tại
                if (!string.IsNullOrEmpty(student.AvatarUrl))
                {
                    var oldAvatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldAvatarPath))
                    {
                        System.IO.File.Delete(oldAvatarPath);
                    }
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var avatarFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
                if (!Directory.Exists(avatarFolder))
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                // Tạo tên file duy nhất
                var avatarFileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var avatarFilePath = Path.Combine(avatarFolder, avatarFileName);

                // Lưu file
                using (var stream = new FileStream(avatarFilePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                student.AvatarUrl = $"/avatars/{avatarFileName}";
            }

            // Xử lý file upload nếu có
            if (resumeFile != null)
            {
                // Kiểm tra file có phải là PDF không
                if (resumeFile.ContentType != "application/pdf")
                {
                    return BadRequest(new { Message = "Chỉ chấp nhận file PDF cho CV." });
                }

                // Kiểm tra kích thước file (giới hạn 5MB)
                if (resumeFile.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File CV không được vượt quá 5MB." });
                }

                // Đọc file thành bytes
                using (var memoryStream = new MemoryStream())
                {
                    await resumeFile.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }

                // Xóa file cũ nếu tồn tại
                if (!string.IsNullOrEmpty(student.ResumeUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ResumeUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Tạo tên file duy nhất
                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(resumeFile.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                student.ResumeUrl = $"/resumes/{fileName}";

                // Nếu có PDF, phân tích CV bằng AI và thay thế hoàn toàn
                var aiStudent = await _geminiService.ClassifyStudentCV(pdfBytes);
                if (aiStudent != null)
                {
                    // Thay thế hoàn toàn bằng dữ liệu từ AI
                    student.Name = aiStudent.Name ?? student.Name;
                    student.GPA = aiStudent.GPA;
                    student.Skills = aiStudent.Skills;
                    student.Archievements = aiStudent.Archievements;
                    student.YearOfStudy = aiStudent.YearOfStudy;
                    student.Major = aiStudent.Major;
                    student.Languages = aiStudent.Languages;
                    student.Certifications = aiStudent.Certifications;
                    student.Experiences = aiStudent.Experiences;
                    student.Projects = aiStudent.Projects;
                    student.Educations = aiStudent.Educations;
                }
            }

            // Cập nhật OpenToWork nếu có trong DTO
            if (studentDto.OpenToWork.HasValue)
            {
                student.OpenToWork = studentDto.OpenToWork.Value;
            }

            if (resumeFile == null)
            {
                // Không có PDF, chỉ update các field có giá trị từ DTO
                if (!string.IsNullOrEmpty(studentDto.Name)) student.Name = studentDto.Name;
                if (!string.IsNullOrEmpty(studentDto.GPA)) student.GPA = studentDto.GPA;
                if (!string.IsNullOrEmpty(studentDto.Skills)) student.Skills = studentDto.Skills;
                if (!string.IsNullOrEmpty(studentDto.Archievements)) student.Archievements = studentDto.Archievements;
                if (!string.IsNullOrEmpty(studentDto.YearOfStudy)) student.YearOfStudy = studentDto.YearOfStudy;
                if (!string.IsNullOrEmpty(studentDto.Major)) student.Major = studentDto.Major;
                if (!string.IsNullOrEmpty(studentDto.Languages)) student.Languages = studentDto.Languages;
                if (!string.IsNullOrEmpty(studentDto.Certifications)) student.Certifications = studentDto.Certifications;
                if (!string.IsNullOrEmpty(studentDto.Experiences)) student.Experiences = studentDto.Experiences;
                if (!string.IsNullOrEmpty(studentDto.Projects)) student.Projects = studentDto.Projects;
                if (!string.IsNullOrEmpty(studentDto.Educations)) student.Educations = studentDto.Educations;
            }

            student.UpdatedAt = DateTime.UtcNow;
            student.UpdatedBy = userId;

            // Cập nhật lại embedding
            student.EmBeddings = await _geminiService.GetStudentEmbedding(student);

            // Debug logging
            Console.WriteLine($"Updating student ID: {student.Id}, Name: {student.Name}");
            
            _unitOfWork.StudentInfors.Update(student);
            var saveResult = await _unitOfWork.SaveChangesAsync();
            
            Console.WriteLine($"Save result: {saveResult} records affected");

            var response = new StudentInforResponseDto
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
                UpdatedAt = student.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost("delete/{id}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> DeleteStudentInfor(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var student = await _unitOfWork.StudentInfors.GetByIdAsync(id);

            if (student == null || student.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin sinh viên." });
            }

            if (userRole != "Admin" && student.UserId != userId)
            {
                return Forbid();
            }

            student.DeletedAt = DateTime.UtcNow;
            student.UpdatedBy = userId;

            _unitOfWork.StudentInfors.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/StudentInfors/JobSuggestions/{studentInforId}
        [HttpGet("JobSuggestions/{studentInforId}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<ActionResult<IEnumerable<JobSuggestionDto>>> GetJobSuggestions(int studentInforId, [FromQuery] int top = 10)
        {
            var student = await _unitOfWork.StudentInfors.GetByIdAsync(studentInforId);
            
            if (student == null || student.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin sinh viên." });
            }

            // Kiểm tra quyền truy cập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin" && student.UserId != userId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(student.EmBeddings))
            {
                return BadRequest(new { Message = "Sinh viên chưa có embedding. Vui lòng cập nhật hồ sơ." });
            }

            var studentEmbedding = JsonSerializer.Deserialize<List<double>>(student.EmBeddings);
            if (studentEmbedding == null || studentEmbedding.Count == 0)
            {
                return BadRequest(new { Message = "Embedding không hợp lệ." });
            }

            // Lấy tất cả jobs đang active
            var jobs = await _unitOfWork.Jobs.GetActiveJobsAsync("Active", null, null);

            var jobSuggestions = new List<JobSuggestionDto>();

            foreach (var job in jobs)
            {
                if (string.IsNullOrWhiteSpace(job.Embeddings))
                    continue;

                var jobEmbedding = JsonSerializer.Deserialize<List<double>>(job.Embeddings);
                if (jobEmbedding == null || jobEmbedding.Count == 0)
                    continue;

                var similarity = _geminiService.CosineSimilarity(studentEmbedding, jobEmbedding);

                jobSuggestions.Add(new JobSuggestionDto
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
                    UpdatedAt = job.UpdatedAt,
                    SimilarityScore = similarity
                });
            }

            // Sắp xếp theo độ tương đồng giảm dần và lấy top kết quả
            var topSuggestions = jobSuggestions
                .OrderByDescending(j => j.SimilarityScore)
                .Take(top)
                .ToList();

            return Ok(topSuggestions);
        }
    }
}
