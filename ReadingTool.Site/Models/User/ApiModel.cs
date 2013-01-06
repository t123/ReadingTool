using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ReadingTool.Core.Attributes;

namespace ReadingTool.Site.Models.User
{
    [Description("API Settings")]
    public class ApiModel
    {
        [Display(Name = "Enable API access?")]
        [Tip("You can only use the API if you enable access here.")]
        public bool IsEnabled { get; set; }
        [Display(Name = "Create a new key")]
        public bool CreateNewKey { get; set; }
        public string ApiKey { get; set; }
    }
}