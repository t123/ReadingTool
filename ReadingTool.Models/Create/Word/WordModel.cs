#region License
// WordModel.cs is part of ReadingTool.Models
// 
// ReadingTool.Models is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Models is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Models. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.ComponentModel;
using MongoDB.Bson;
using ReadingTool.Common.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Models.Create.Word
{
    public class WordModel
    {
        public ObjectId WordId { get; set; }

        [DisplayName("Language")]
        public ObjectId LanguageId { get; set; }
        private string _wordPhrase;

        [DisplayName("Word")]
        public string WordPhrase
        {
            get { return _wordPhrase; }
            set
            {
                _wordPhrase = value;
                if (value == null)
                {
                    return;
                }

                Length = _wordPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }
        }

        public WordState State { get; set; }
        [Help("Your definition of the word.")]
        public string Definition { get; set; }

        [DisplayName("Base word")]
        [Help("This can anything you want, but generally you try use the infinitive of the verb and the nominative singular for nouns. " +
            "The purpose is to group all the different forms of a word so they can easily be found.")]
        public string BaseWord { get; set; }

        [Help("The romanised text of the word.")]
        public string Romanisation { get; set; }

        [Help("Any tags for your words. Tags are comma separated.")]
        public string Tags { get; set; }

        [Help("An example sentence for your word.")]
        public string Sentence { get; set; }
        public int Length { get; private set; }

        public int Box { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string FullDefinition
        {
            get { return string.Join("\n", new string[] { BaseWord, Romanisation, Definition }).Trim(); }
        }
    }
}