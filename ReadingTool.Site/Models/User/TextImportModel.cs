using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class TextImportModel
    {
        [Display(Name="JSON File")]
        public HttpPostedFileBase File { get; set; }
    }
}