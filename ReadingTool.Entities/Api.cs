using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Entities
{
    public class Api
    {
        [Required]
        public bool IsEnabled { get; set; }

        [StringLength(40)]
        public string ApiKey { get; set; }
    }
}