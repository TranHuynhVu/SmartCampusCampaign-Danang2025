using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCD_2025_BE.Entities.Domains;

public class StudentInfor
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
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
    public string? Educations  { get; set; }
    public string? EmBeddings { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AppUser User { get; set; }
}
