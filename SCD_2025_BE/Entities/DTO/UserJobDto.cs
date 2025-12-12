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
        public string? StudentName { get; set; }
        public string? StudentAvatar { get; set; }
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? Type { get; set; }
        public string Status { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
