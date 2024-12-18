using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Service_Library.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(50, ErrorMessage = "Max 50 Characters allowed.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50, ErrorMessage = "Max 50 Characters allowed.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(50, ErrorMessage = "Max 50 Characters allowed.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MaxLength(100, ErrorMessage = "Max 100 Characters allowed.")]
        public string Password { get; set; }
        public string Role { get; set; } = "User"; // Default role
    }
}
