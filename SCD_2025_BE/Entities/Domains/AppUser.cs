using Microsoft.AspNetCore.Identity;

namespace SCD_2025_BE.Entities.Domains
{
    public class AppUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        
        // Navigation properties
        public StudentInfor? StudentInfor { get; set; }
        public CompanyInfor? CompanyInfor { get; set; }
    }
}
