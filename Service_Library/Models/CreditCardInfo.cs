using System.ComponentModel.DataAnnotations;
using Service_Library.Entities;

namespace Service_Library.Models
{
    public class CreditCardInfo
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID must be 9 digits")]
        public string PersonalId { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "Invalid credit card number format")]
        public string CreditCardNumber { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([2-9]\d)$", ErrorMessage = "Valid date must be in format MM/YY")]
        public string ValidDate { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVC must be 3 digits")]
        public string CVC { get; set; }

        public string Note { get; set; }

        public int UserAccountId { get; set; }
        public UserAccount UserAccount { get; set; }
    }
} 