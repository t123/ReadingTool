using System.ComponentModel;

namespace ReadingTool.Common
{
    public enum TermState
    {
        [Description("Known")]
        Known = 1,

        [Description("Not Known")]
        NotKnown = 2,

        [Description("Ignored")]
        Ignore = 4,

        [Description("Not Seen")]
        NotSeen = 3
    }
}