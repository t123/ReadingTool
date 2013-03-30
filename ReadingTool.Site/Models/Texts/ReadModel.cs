#region License
// ReadModel.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Languages;

namespace ReadingTool.Site.Models.Texts
{
    public class ReadModel
    {
        public bool AsParallel { get; set; }
        public string ParsedText { get; set; }
        public string ApiDomain { get; set; }
        public Tuple<Guid, Guid> PagedTexts { get; set; }

        public LanguageViewModel Language { get; set; }
        public LanguageViewModel Language2 { get; set; }
        public AccountModel.UserModel User { get; set; }
        public TextViewModel Text { get; set; }
    }
}