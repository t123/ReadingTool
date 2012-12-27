using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.User
{
    public class PasswordChangeViewModel
    {
        [Display(Name = "Current Password", Order = 1)]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password", Order = 2)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm New Password", Order = 3)]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}