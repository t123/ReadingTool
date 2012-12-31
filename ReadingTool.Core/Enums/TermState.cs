using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Core.Enums
{
    public enum TermState
    {
        [Description("Known")]
        Known = 1,

        [Description("Unknown")]
        Unknown = 2,

        [Description("Not Seen")]
        NotSeen = 3,

        [Description("Ignored")]
        Ignored = 4
    }
}
