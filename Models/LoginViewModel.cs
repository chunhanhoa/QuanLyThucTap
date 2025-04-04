using System.ComponentModel.DataAnnotations;

namespace ABC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
