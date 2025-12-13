using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class JobDto
    {
        [Required(ErrorMessage = "Tiêu đề công việc không được để trống.")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả công việc không được để trống.")]
        public string Description { get; set; }

        public string? SalaryRange { get; set; }
        public string? DayOfWeek { get; set; }
        public string? TimeOfDay { get; set; }
        public string? Benefits { get; set; }
        public string? Requirements { get; set; }
        public string? NiceToHave { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression("^(Active|Inactive|Closed)$", ErrorMessage = "Trạng thái phải là Active, Inactive hoặc Closed.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Danh mục không được để trống.")]
        public int CategoryId { get; set; }
    }

    public class JobResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? SalaryRange { get; set; }
        public string? DayOfWeek { get; set; }
        public string? TimeOfDay { get; set; }
        public string? Benefits { get; set; }
        public string? Requirements { get; set; }
        public string? NiceToHave { get; set; }
        public int CompanyInforId { get; set; }
        public string ? CompanyLogo { get; set; }
        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class JobSuggestionDto : JobResponseDto
    {
        public double SimilarityScore { get; set; }
    }
}
