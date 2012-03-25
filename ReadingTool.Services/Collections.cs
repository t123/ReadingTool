#region License
// Collections.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public class Collections
    {
        public const string Tokens = Token.CollectionName;
        public const string APIRequests = ApiRequest.CollectionName;
        public const string Emails = Email.CollectionName;
        public const string EmailTemplates = EmailTemplate.CollectionName;
        public const string GroupShareNotices = GroupShareNotice.DbCollectionName;
        public const string Messages = Message.CollectionName;
        public const string Users = User.CollectionName;
        public const string PasswordResets = PasswordReset.CollectionName;
        public const string SystemLanguages = SystemLanguage.CollectionName;
        public const string Languages = Language.CollectionName;
        public const string Words = Word.CollectionName;
        public const string TextParsers = TextParser.CollectionName;
        public const string Tasks = SystemTask.CollectionName;
        public const string Groups = Group.CollectionName;
        public const string GroupMembers = GroupMember.CollectionName;
        public const string Items = Item.DbCollectionName;
        public const string Xsl = Entities.Xsl.CollectionName;

        public const string ParsingTimes = @"ParsingTimes";
        public const string SystemSettings = @"SystemSettings";
        public const string Logs = @"Logs_Site";
    }
}