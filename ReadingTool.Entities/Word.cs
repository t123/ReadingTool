#region License
// Word.cs is part of ReadingTool.Entities
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
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class Word
    {
        public const string CollectionName = @"Words";

        [BsonId]
        public ObjectId WordId { get; set; }
        public ObjectId LanguageId { get; set; }
        public ObjectId Owner { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ObjectId ItemId { get; set; }
        public string LanguageName { get; set; }
        public string LanguageColour { get; set; }
        public ObjectId SystemLanguageId { get; set; }
        public DateTime? LastReview { get; set; }
        public int Resets { get; set; }

        private string _wordPhrase;
        public string WordPhrase
        {
            get { return _wordPhrase; }
            set
            {
                _wordPhrase = (value ?? "").Trim();
                WordPhraseLower = _wordPhrase.ToLowerInvariant();
                Length = _wordPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }
        }

        public string WordPhraseLower { get; private set; }
        public WordState State { get; set; }
        public string Definition { get; set; }

        private string _baseWord;
        public string BaseWord
        {
            get { return _baseWord; }
            set
            {
                _baseWord = (value ?? "").Trim();
                BaseWordLower = (_baseWord ?? "").ToLowerInvariant();
            }
        }

        public string BaseWordLower { get; private set; }

        public string Romanisation { get; set; }

        private string[] _tags;
        public string[] Tags { get { return _tags ?? new string[0]; } set { _tags = value; } }
        public int Box { get; set; }
        public DateTime NextReview { get; set; }
        public string Sentence { get; set; }
        public int Length { get; private set; }

        public Word Clone()
        {
            return new Word()
            {
                WordId = this.WordId,
                LanguageId = this.LanguageId,
                Tags = this.Tags,
                Created = this.Created,
                Modified = this.Modified,
                Owner = this.Owner,
                State = this.State,
                Sentence = this.Sentence,
                BaseWord = this.BaseWord,
                Romanisation = this.Romanisation,
                NextReview = this.NextReview,
                Length = this.Length,
                Box = this.Box,
                Definition = this.Definition,
                //ItemId = this.ItemId,
                WordPhrase = this.WordPhrase,
                WordPhraseLower = this.WordPhraseLower,
                LanguageName = this.LanguageName,
                LanguageColour = this.LanguageColour
            };
        }

        public bool ShouldSerializeTags()
        {
            return Tags != null && Tags.Length > 0;
        }

        [BsonIgnore]
        public string FullDefinition
        {
            get
            {
                return string.IsNullOrEmpty(Romanisation)
                    ? string.Join("\n", new string[] { BaseWord, Definition }).Trim()
                    : string.Join("\n", new string[] { BaseWord, Romanisation, Definition }).Trim();
            }
        }
    }
}