using System.ComponentModel.DataAnnotations;

namespace SCD_2025_BE.Entities.DTO
{
    public class RegisterRequestDto
    {

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ thường, 1 chữ hoa, 1 số và 1 ký tự đặc biệt.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [RegularExpression("^(Student|Company)$", ErrorMessage = "Vai trò phải là Student hoặc Company.")]
        public string Role { get; set; }
    }
}
