using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCD_2025_BE.Entities.Domains;

public class Job
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? SalaryRange { get; set; }
    public string? DayOfWeek { get; set; }
    public string? TimeOfDay { get; set; }
    public string? Benefits { get; set; }
    public string? Requirements { get; set; }
    public string? NiceToHave { get; set; }
    [ForeignKey(nameof(CompanyInfor))]
    public int CompanyInforId { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; }
    public string? Embeddings { get; set; }
    public int CategoryId { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Category Category { get; set; }
    public CompanyInfor CompanyInfor { get; set; }
}
