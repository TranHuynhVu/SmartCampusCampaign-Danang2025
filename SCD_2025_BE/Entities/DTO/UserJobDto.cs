using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class UserJobDto
    {
        [Required(ErrorMessage = "Job ID không được để trống.")]
        public int JobId { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression("^(Applied|Reviewing|Accepted|Rejected|Withdrawn)$", 
            ErrorMessage = "Trạng thái phải là Applied, Reviewing, Accepted, Rejected hoặc Withdrawn.")]
        public string Status { get; set; }
    }

    public class CompanyRecruitDto
    {
        [Required(ErrorMessage = "User ID không được để trống.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Job ID không được để trống.")]
        public int JobId { get; set; }
    }

    public class UserJobUpdateDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression("^(Applied|Reviewing|Accepted|Rejected|Withdrawn)$", 
            ErrorMessage = "Trạng thái phải là Applied, Reviewing, Accepted, Rejected hoặc Withdrawn.")]
        public string Status { get; set; }
    }

    public class UserJobResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string Status { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
