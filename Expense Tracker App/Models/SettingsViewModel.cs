using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker_App.Models
{
    public class SettingsViewModel
    {
        [EmailAddress]
        [Required]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [Phone]
        [MaxLength(15)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Theme { get; set; }

        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "Push Notifications")]
        public bool PushNotifications { get; set; }

        // Password change properties
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
} 