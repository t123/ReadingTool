using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Common
{
    public enum GroupType
    {
        Public = 1,
        Private = 2
    }

    public enum MembershipType
    {
        None = 0,
        Owner = 1,
        Moderator = 2,
        Member = 3,
        Pending = 4,
        Banned = 5
    }
}
