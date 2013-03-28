using System.ComponentModel.DataAnnotations;

namespace ReadingTool.Site.Models.Admin
{
    public class SystemLanguageModel
    {
        public int SystemLanguageId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Code { get; set; }
    }
}