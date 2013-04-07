using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Attributes
{
    public class TipAttribute : Attribute
    {
        public string Description { get; set; }

        public TipAttribute(string description)
        {
            Description = description;
        }
    }
}