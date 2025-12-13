using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class CompanyInforDto
    {
        [Required(ErrorMessage = "Tên công ty không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên công ty tối đa 200 ký tự.")]
        public string CompanyName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự.")]
        public string? Descriptions { get; set; }

        [Url(ErrorMessage = "Website không hợp lệ.")]
        public string? CompanyWebsite { get; set; }

        public string? LogoUrl { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? Description { get; set; }
    }

    public class CompanyInforResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string? Descriptions { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? LogoUrl { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
