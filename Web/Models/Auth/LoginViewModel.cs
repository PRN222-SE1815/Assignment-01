using System.ComponentModel.DataAnnotations;

namespace Web.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, ErrorMessage = "Username không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
