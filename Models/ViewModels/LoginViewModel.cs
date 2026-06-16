using System.ComponentModel.DataAnnotations;

namespace KT_LTWEB.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên tài khoản không được để trống")]
        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ tài khoản")]
        public bool RememberMe { get; set; }
    }
}
