#region License
// FilterKeys.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using ReadingTool.Common.Enums;

namespace ReadingTool.Common.Keys
{
    public class FilterKeys
    {
        public static string[] GroupNames = new[] { Group.YOUR_GROUPS, Group.MANAGE, Group.INVITATIONS, Group.PUBLIC_GROUPS };
        public static string[] GroupManageTypes = new[]
                                                      {
                                                          GroupMembershipType.Owner.ToString(), 
                                                          GroupMembershipType.Moderator.ToString(), 
                                                          GroupMembershipType.Member.ToString(), 
                                                          GroupMembershipType.Invitation.ToString(), 
                                                          GroupMembershipType.Request.ToString(), 
                                                          GroupMembershipType.Banned.ToString()
                                                      };

        public static string[] MessageNames = new[] { Message.INBOX, Message.STARRED, Message.UNREAD, Message.SENT };

        public class Message
        {
            public static string INBOX = @"inbox";
            public static string STARRED = @"starred";
            public static string UNREAD = @"unread";
            public static string SENT = @"sent";
        }

        public class Group
        {
            public static string YOUR_GROUPS = @"your groups";
            public static string MANAGE = @"manage";
            public static string INVITATIONS = @"invitations";
            public static string PUBLIC_GROUPS = @"public groups";
        }

        public static string[] GroupSearch = new[] { GroupSearchType.TEXTS, GroupSearchType.VIDEOS };

        public class GroupSearchType
        {
            public static string TEXTS = @"texts";
            public static string VIDEOS = @"videos";
        }
    }
}
