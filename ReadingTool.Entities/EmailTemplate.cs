#region License
// EmailTemplate.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using MongoDB.Bson.Serialization.Attributes;

namespace ReadingTool.Entities
{
    public class EmailTemplate
    {
        public const string CollectionName = @"EmailTemplates";

        public static class TemplateNames
        {
            public const string ForgotPassword = @"forgot-password";
            public const string PasswordReset = @"password-reset-confirmation";
        }

        [BsonId]
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Format { get; set; }
    }
}
