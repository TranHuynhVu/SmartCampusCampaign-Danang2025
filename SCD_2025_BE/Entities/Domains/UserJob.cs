using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCD_2025_BE.Entities.Domains;

public class UserJob
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
    public string UserId { get; set; }
    [ForeignKey(nameof(Job))]
    public int JobId { get; set; }
    public string Status { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public AppUser User { get; set; }
    public Job Job { get; set; }
}
