using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ReadingTool.Site.Models.Texts
{
    public class TextImportModel
    {
        [Display(Name = "JSON File")]
        public HttpPostedFileBase File { get; set; }
    }
}