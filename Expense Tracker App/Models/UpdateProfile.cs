using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker_App.Models
{
    public class UpdateProfile
    {
  
            [Required, MaxLength(100)]
            public string UserName { get; set; }

            [Required, MaxLength(150)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }

            [MaxLength(15)]
            public string? PhoneNumber { get; set; }

            [Required, DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

        public IFormFile? ProfileImg { get; set; } // Nhận file ảnh từ form

    }
}
