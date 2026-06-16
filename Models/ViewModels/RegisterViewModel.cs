using System.ComponentModel.DataAnnotations;

namespace KT_LTWEB.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        [StringLength(50, ErrorMessage = "Tên tài khoản phải từ 3 đến 50 ký tự", MinimumLength = 3)]
        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
