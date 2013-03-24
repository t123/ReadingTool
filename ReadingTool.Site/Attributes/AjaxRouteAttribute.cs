﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Attributes
{
    public class AjaxRouteAttribute : Attribute
    {
        public string Name { get; set; }

        public AjaxRouteAttribute()
        {
        }

        public AjaxRouteAttribute(string name)
        {
            Name = name;
        }
    }
}