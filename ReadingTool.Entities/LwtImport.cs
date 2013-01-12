using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core.Attributes;

namespace ReadingTool.Entities
{
    public class LwtImport
    {
        [Display(Name = "Default media URL")]
        [Tip("This will be prepended to the URL specified in LWT.")]
        public string MediaUrl
        {
            get { return _mediaUrl; }
            set
            {
                _mediaUrl = value ?? "";
                if(!_mediaUrl.EndsWith("/")) _mediaUrl += "/";
            }
        }
        private string _mediaUrl;

        [Display(Name = "Test Mode")]
        public bool TestMode { get; set; }
        public Stream File { get; set; }

        public LwtImport()
        {
            TestMode = true;
        }
    }
}
