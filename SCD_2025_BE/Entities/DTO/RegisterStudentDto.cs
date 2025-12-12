using Microsoft.AspNetCore.Http;

namespace SCD_2025_BE.Entities.DTO
{
    public class RegisterStudentDto
    {
        // User fields
        public string Email { get; set; }
        public string Password { get; set; }
        
        // StudentInfor fields - optional if Resume is provided
        public string? Name { get; set; }
        public string? GPA { get; set; }
        public string? Educations { get; set; }
        public string? Skills { get; set; }
        public string? Archievements { get; set; }
        public string? YearOfStudy { get; set; }
        public string? Major { get; set; }
        public string? Languages { get; set; }
        public string? Certifications { get; set; }
        public string? Experiences { get; set; }
        public string? Projects { get; set; }
        
        // Resume file - optional
        public IFormFile? Resume { get; set; }
    }
}
