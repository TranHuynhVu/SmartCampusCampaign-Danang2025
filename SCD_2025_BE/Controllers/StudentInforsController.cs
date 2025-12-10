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
    public class StudentInforsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentInforsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                UpdatedBy = student.UpdatedBy,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentInforResponseDto>> CreateStudentInfor(StudentInforDto studentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingStudent = await _unitOfWork.StudentInfors.GetByUserIdAsync(userId);

            if (existingStudent != null)
            {
                return BadRequest(new { Message = "Bạn đã tạo hồ sơ sinh viên rồi." });
            }

            var student = new StudentInfor
            {
                UserId = userId,
                Name = studentDto.Name,
                ResumeUrl = studentDto.ResumeUrl,
                GPA = studentDto.GPA,
                Skills = studentDto.Skills,
                Archievements = studentDto.Archievements,
                YearOfStudy = studentDto.YearOfStudy,
                Major = studentDto.Major,
                Languages = studentDto.Languages,
                Certifications = studentDto.Certifications,
                Experiences = studentDto.Experiences,
                Projects = studentDto.Projects,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

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
                UpdatedBy = student.UpdatedBy,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };

            return CreatedAtAction(nameof(GetStudentInfor), new { id = student.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UpdateStudentInfor(int id, StudentInforDto studentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var student = await _unitOfWork.StudentInfors.GetByIdAsync(id);

            if (student == null || student.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy thông tin sinh viên." });
            }

            if (student.UserId != userId)
            {
                return Forbid();
            }

            student.Name = studentDto.Name;
            student.ResumeUrl = studentDto.ResumeUrl;
            student.GPA = studentDto.GPA;
            student.Skills = studentDto.Skills;
            student.Archievements = studentDto.Archievements;
            student.YearOfStudy = studentDto.YearOfStudy;
            student.Major = studentDto.Major;
            student.Languages = studentDto.Languages;
            student.Certifications = studentDto.Certifications;
            student.Experiences = studentDto.Experiences;
            student.Projects = studentDto.Projects;
            student.UpdatedAt = DateTime.UtcNow;
            student.UpdatedBy = userId;

            _unitOfWork.StudentInfors.Update(student);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
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
    }
}
