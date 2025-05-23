﻿using System.ComponentModel.DataAnnotations;

namespace Service_Library.Models
{
    public class RegistrationViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Personal ID")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID must be 9 digits")]
        public string PersonalId { get; set; }

        [Required]
        [Display(Name = "Credit Card Number")]
        [RegularExpression(@"^\d{4}\s?\d{4}\s?\d{4}\s?\d{4}$", ErrorMessage = "Invalid credit card number format")]
        public string CreditCardNumber { get; set; }

        [Required]
        [Display(Name = "Valid Until (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([2-9]\d)$", ErrorMessage = "Valid date must be in format MM/YY")]
        public string ValidDate { get; set; }

        [Required]
        [Display(Name = "CVC")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVC must be 3 digits")]
        public string CVC { get; set; }

        public string Note { get; set; }
    }
}
