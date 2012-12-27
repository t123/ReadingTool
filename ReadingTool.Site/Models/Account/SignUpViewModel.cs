using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Account
{
    public class SignUpViewModel
    {
        [Required]
        [Display(Name = "Username", Order = 1)]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }
    }
}