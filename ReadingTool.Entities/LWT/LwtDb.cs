#region License
// LwtDb.cs is part of ReadingTool.Entities
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

using System;
using MongoDB.Bson;

namespace ReadingTool.Entities.LWT
{
    public class ArchivedTexts
    {
        public int? AtId { get; set; }
        public int? AtLgID { get; set; }
        public string AtTitle { get; set; }
        public string AtText { get; set; }
        public string AtAudioURI { get; set; }
    }

    public class ArchTextTags
    {
        public int? AgAtID { get; set; }
        public int? AgT2ID { get; set; }
    }

    public class Languages
    {
        public ObjectId Id { get; set; }


        public int? LgID { get; set; }
        public string LgName { get; set; }
        public string LgDict1URI { get; set; }
        public string LgDict2URI { get; set; }
        public string LgGoogleTranslateURI { get; set; }
        public string LgGoogleTTSURI { get; set; }
        public int? LgTextSize { get; set; }
        public string LgCharacterSubstitutions { get; set; }
        public string LgRegexpSplitSentences { get; set; }
        public string LgExceptionsSplitSentences { get; set; }
        public string LgRegexpWordCharacters { get; set; }
        public bool? LgRemoveSpaces { get; set; }
        public bool? LgSplitEachChar { get; set; }
        public bool? LgRightToLeft { get; set; }
    }

    public class Sentences
    {
        public int? SeID { get; set; }
        public int? SeLgID { get; set; }
        public int? SeTxID { get; set; }
        public int? SeOrder { get; set; }
        public string SeText { get; set; }
    }

    public class Tags
    {
        public int? TgID { get; set; }
        public string TgText { get; set; }
        public string TgComment { get; set; }
    }

    public class Tags2
    {
        public int? T2ID { get; set; }
        public string T2Text { get; set; }
        public string T2Comment { get; set; }
    }

    public class TextItems
    {
        public int? TiID { get; set; }
        public int? TiLgID { get; set; }
        public int? TiTxID { get; set; }
        public int? TiSeID { get; set; }
        public int? TiOrder { get; set; }
        public int? TiWordCount { get; set; }
        public string TiText { get; set; }
        public string TiTextLC { get; set; }
        public bool? TiIsNotWord { get; set; }
    }

    public class Texts
    {
        public int? TxID { get; set; }
        public int? TxLgID { get; set; }
        public string TxTitle { get; set; }
        public string TxText { get; set; }
        public string TxAudioURI { get; set; }
    }

    public class TextTags
    {
        public int? TtTxID { get; set; }
        public int? TtT2ID { get; set; }
    }

    public class Words
    {
        public int? WoID { get; set; }
        public int? WoLgID { get; set; }
        public string WoText { get; set; }
        public string WoTextLC { get; set; }
        public int? WoStatus { get; set; }
        public string WoTranslation { get; set; }
        public string WoRomanization { get; set; }
        public string WoSentence { get; set; }
        public DateTime? WoCreated { get; set; }
        public DateTime? WoStatusChanged { get; set; }
        public double? WoTodayScore { get; set; }
        public double? WoTomorrowScore { get; set; }
        public double? WoRandom { get; set; }
    }

    public class WordTags
    {
        public int? WtWoID { get; set; }
        public int? WtTgID { get; set; }
    }
}