using System;

namespace ReadingTool.Core.Attributes
{
    public class TipAttribute : Attribute
    {
        public TipAttribute()
            : this("")
        {
        }

        public TipAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
    }
}