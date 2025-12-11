using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class StudentInforDto
    {
        [Required(ErrorMessage = "Tên sinh viên không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự.")]
        public string Name { get; set; }

        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "GPA phải là số.")]
        public string? GPA { get; set; }

        public string? Skills { get; set; }
        public string? Archievements { get; set; }
        public string? YearOfStudy { get; set; }

        [StringLength(100, ErrorMessage = "Chuyên ngành tối đa 100 ký tự.")]
        public string? Major { get; set; }

        public string? Languages { get; set; }
        public string? Certifications { get; set; }
        public string? Experiences { get; set; }
        public string? Projects { get; set; }
    }

    public class StudentInforResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string? ResumeUrl { get; set; }
        public string? GPA { get; set; }
        public string? Skills { get; set; }
        public string? Archievements { get; set; }
        public string? YearOfStudy { get; set; }
        public string? Major { get; set; }
        public string? Languages { get; set; }
        public string? Certifications { get; set; }
        public string? Experiences { get; set; }
        public string? Projects { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CandidateSuggestionDto : StudentInforResponseDto
    {
        public double SimilarityScore { get; set; }
    }
}
