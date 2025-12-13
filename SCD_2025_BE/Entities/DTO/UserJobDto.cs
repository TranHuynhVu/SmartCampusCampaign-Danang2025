using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class UserJobDto
    {
        [Required(ErrorMessage = "Job ID không được để trống.")]
        public int JobId { get; set; }
    }

    public class CompanyInvitationDto
    {
        [Required(ErrorMessage = "Student User ID không được để trống.")]
        public string StudentUserId { get; set; }

        [Required(ErrorMessage = "Job ID không được để trống.")]
        public int JobId { get; set; }
    }

    public class UserJobResponseStatusDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression("^(Accepted|Rejected)$", 
            ErrorMessage = "Trạng thái phải là Accepted hoặc Rejected.")]
        public string Status { get; set; }
    }

    public class UserJobResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        
        // Student Information
        public string? StudentName { get; set; }
        public string? StudentAvatar { get; set; }
        public string? StudentEmail { get; set; }
        public string? StudentGPA { get; set; }
        public string? StudentSkills { get; set; }
        public string? StudentYearOfStudy { get; set; }
        public string? StudentMajor { get; set; }
        public string? StudentResumeUrl { get; set; }
        public string? StudentLanguages { get; set; }
        public string? StudentExperiences { get; set; }
        public bool? StudentOpenToWork { get; set; }
        
        // Job Information
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        public string? JobSalaryRange { get; set; }
        public string? JobLocation { get; set; }
        public string? JobDayOfWeek { get; set; }
        public string? JobTimeOfDay { get; set; }
        public string? JobBenefits { get; set; }
        public string? JobRequirements { get; set; }
        public string? JobStatus { get; set; }
        
        // Company Information
        public int? CompanyInforId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyDescriptions { get; set; }
        
        // Category Information
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        // UserJob Information
        public string? Type { get; set; }
        public string Status { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
