using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(50, ErrorMessage = "Max 50 Characters allowed.")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please Enter Valid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(100, ErrorMessage = "Max 100 Characters allowed.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
