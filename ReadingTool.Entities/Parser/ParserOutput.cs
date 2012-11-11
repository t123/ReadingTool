#region License
// ParserOutput.cs is part of ReadingTool.Entities
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

using System.Collections.Generic;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;

namespace ReadingTool.Entities.Parser
{
    public class ParserOutput
    {
        public class MediaControlData
        {
            public bool IsEnabled { get; set; }
            public bool AutoPause { get; set; }
            public decimal SecondsToRewind { get; set; }
            public int RewindToBeginning { get; set; }
            public int Rewind { get; set; }
            public int PlayPause { get; set; }
            public int Stop { get; set; }
            public int FastForward { get; set; }
        }

        public class DictionaryData
        {
            public string Name { get; set; }
            public string WindowName { get; set; }
            public string Url { get; set; }
            public string Encoding { get; set; }
        }

        public class LanguageData
        {
            public string LanguageId { get; set; }
            public string Name { get; set; }
            public bool HasRomanisationField { get; set; }
            public string TranslateUrl { get; set; }
            public bool IsRtlLanguage { get; set; }
            public string ModalBehaviour { get; set; }
            public bool AutoOpenDictionary { get; set; }
            public string AutoDictionaryUrl { get; set; }
            public string AutoDictionaryWindowName { get; set; }
            public bool RemoveSpaces { get; set; }
            public bool KeepFocus { get; set; }
        }

        public class ItemData
        {
            public bool AsParallel { get; set; }
            public string ItemId { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public string CollectionName { get; set; }
            public int? CollectionNo { get; set; }
            public string NextItem { get; set; }
            public string PreviousItem { get; set; }
            public bool IsParallel { get; set; }
            public ItemType ItemType { get; set; }
        }

        public class VideoPlaybackData
        {
            public string ElementId { get; set; }
            public decimal FromSeconds { get; set; }
            public decimal ToSeconds { get; set; }
            public bool L1 { get; set; }
        }

        public class StyleData
        {
            public string Known { get; set; }
            public string Unknown { get; set; }
            public string Ignored { get; set; }
            public string NotSeen { get; set; }
            public string Space { get; set; }
            public string Punctuation { get; set; }

            public StyleData()
            {
                Known = EnumHelper.GetAlternateName(WordState.Known);
                Unknown = EnumHelper.GetAlternateName(WordState.Unknown);
                Ignored = EnumHelper.GetAlternateName(WordState.Ignored);
                NotSeen = EnumHelper.GetAlternateName(WordState.NotSeen);
                Space = "wsx";
                Punctuation = "pcx";
            }
        }

        public string ParsedHtml { get; set; }
        public bool ShareWords { get; set; }
        public LanguageData Language { get; set; }
        public ItemData Item { get; set; }
        public IList<DictionaryData> Dictionaries { get; set; }
        public MediaControlData MediaControls { get; set; }
        public StyleData Style { get; set; }
        public IList<VideoPlaybackData> VideoPlayback { get; set; }
        public Style UserStyle { get; set; }

        public ParserOutput()
        {
            Dictionaries = new List<DictionaryData>();
            Style = new StyleData();
        }
    }
}