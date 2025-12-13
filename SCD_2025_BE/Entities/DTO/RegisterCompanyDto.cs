namespace SCD_2025_BE.Entities.DTO
{
    public class RegisterCompanyDto
    {
        // User fields
        public string Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
        
        // CompanyInfor fields
        public string CompanyName { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? Location { get; set; }
        public string? Descriptions { get; set; }
    }
}
