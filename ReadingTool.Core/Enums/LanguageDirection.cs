using System.ComponentModel;

namespace ReadingTool.Core.Enums
{
    public enum LanguageDirection : short
    {
        [Description("Left To Right")]
        LTR = 1,
        
        [Description("Right To Left")]
        RTL = 2
    }
}