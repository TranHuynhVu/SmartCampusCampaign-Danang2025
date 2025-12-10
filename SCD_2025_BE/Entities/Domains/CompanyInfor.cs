using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCD_2025_BE.Entities.Domains;

public class CompanyInfor
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    public string? Descriptions { get; set; }
    public string CompanyName { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? LogoUrl { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AppUser User { get; set; }
}
