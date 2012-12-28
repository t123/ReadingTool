using System.Collections.Generic;
using ReadingTool.Core.Enums;

namespace ReadingTool.Core
{
    public class Constants
    {
        public class Messages
        {
            public const string FORM_FAIL = "Please correct the errors in the form below";
            public const string FORM_ADD = "{0} saved";
            public const string FORM_UPDATE = "{0} saved";
            public const string FORM_DELETE = "{0} deleted";
            public const string FORM_NOT_FOUND= "{0} not found";
        }

        public class Roles
        {
            public const string ADMIN = "ADMIN";
            public const string WEB = "WEB";
        }

        public class CacheKeys
        {
            public const string XSL = @"__XSL";
        }

        public class TermStates
        {
            public static string KNOWN = @"knx";
            public static string UNKNOWN = @"nkx";
            public static string NOTSEEN = @"nsx";
            public static string IGNORED = @"igx";
            public static string PUNCTUATION = @"pcx";
            public static string SPACE = @"wsx";
        }

        public static readonly Dictionary<TermState, string> TermStatesToNames = new Dictionary<TermState, string>()
            {
                {TermState.Unknown, Constants.TermStates.UNKNOWN},
                {TermState.Known, Constants.TermStates.KNOWN},
                {TermState.NotSeen, Constants.TermStates.NOTSEEN},
                {TermState.Ignored, Constants.TermStates.IGNORED}
            };

        public static readonly Dictionary<string, TermState> NamesToTermStates = new Dictionary<string, TermState>()
            {
                {Constants.TermStates.KNOWN, TermState.Known},
                {Constants.TermStates.UNKNOWN, TermState.Unknown},
                {Constants.TermStates.NOTSEEN, TermState.NotSeen},
                {Constants.TermStates.IGNORED, TermState.Ignored}
            };
    }
}
