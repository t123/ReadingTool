#region License
// BaseParserService.cs is part of ReadingTool.Services
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

using System.Collections.Generic;
using MongoDB.Bson;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Parser;

namespace ReadingTool.Services
{
    public interface IParserService
    {
        ParserOutput Parse(ParserInput input);
    }

    public abstract class BaseParserService : IParserService
    {
        private readonly IItemService _itemService;
        protected int _maxWords;
        protected ParserOutput _output;
        protected ParsingWord[] _words;
        protected ParsingWord[] _singleWords;
        protected Dictionary<WordState, string> _wordStates = new Dictionary<WordState, string>()
                                                       {
                                                           {WordState.Unknown, EnumHelper.GetAlternateName(WordState.Unknown)},
                                                           {WordState.Known, EnumHelper.GetAlternateName(WordState.Known)},
                                                           {WordState.NotSeen, EnumHelper.GetAlternateName(WordState.NotSeen)},
                                                           {WordState.Ignored, EnumHelper.GetAlternateName(WordState.Ignored)}
                                                       };

        public BaseParserService(IItemService itemService, SystemSystemValues values)
        {
            _maxWords = values.Site.MaxWordsParsingException;
            _itemService = itemService;
            _output = new ParserOutput();
        }

        public abstract ParserOutput Parse(ParserInput input);

        public void Init(ParserInput input)
        {
            _output.ShareWords = input.User.ShareWords;

            _output.Language = new ParserOutput.LanguageData()
                                   {
                                       LanguageId = input.Language.LanguageId.ToString(),
                                       Name = input.Language.Name,
                                       HasRomanisationField = input.Language.HasRomanisationField,
                                       TranslateUrl = input.Language.TranslateUrl,
                                       IsRtlLanguage = input.Language.IsRtlLanguage,
                                       ModalBehaviour = input.Language.ModalBehaviour.ToString(),
                                       AutoOpenDictionary = !string.IsNullOrEmpty(input.Language.DefaultDictionary),
                                       RemoveSpaces = input.Language.RemoveSpaces,
                                       KeepFocus = input.Language.KeepFocus
                                   };

            if(input.Language.Dictionaries != null)
            {
                foreach(var dictionary in input.Language.Dictionaries)
                {
                    if(input.Language.DefaultDictionary == dictionary.Name)
                    {
                        _output.Language.AutoDictionaryUrl = dictionary.Url;
                        _output.Language.AutoDictionaryWindowName = dictionary.WindowName;
                    }

                    _output.Dictionaries.Add(new ParserOutput.DictionaryData()
                                                 {
                                                     Name = dictionary.Name,
                                                     WindowName = dictionary.WindowName,
                                                     Url = dictionary.Url
                                                 }
                        );
                }
            }

            var nextTextId = _itemService.NextItemId(input.Item);
            var previousTextId = _itemService.PreviousItemId(input.Item);
            _output.Item = new ParserOutput.ItemData()
                               {
                                   ItemId = input.Item.ItemId.ToString(),
                                   Title = input.Item.Title,
                                   Url = input.Item.Url,
                                   CollectionNo = input.Item.CollectionNo,
                                   CollectionName = input.Item.CollectionName,
                                   NextItem = nextTextId == ObjectId.Empty ? "" : nextTextId.ToString(),
                                   PreviousItem = previousTextId == ObjectId.Empty ? "" : previousTextId.ToString(),
                                   IsParallel = input.Item.IsParallel,
                                   AsParallel = input.AsParallel,
                                   ItemType = input.Item.ItemType
                               };

            var user = input.User;

            if(user.MediaControl == null)
            {
                _output.MediaControls = new ParserOutput.MediaControlData()
                                            {
                                                IsEnabled = false,
                                                AutoPause = false,
                                                FastForward = -1,
                                                SecondsToRewind = 0,
                                                PlayPause = -1,
                                                Rewind = -1,
                                                RewindToBeginning = -1,
                                                Stop = -1
                                            };
            }
            else
            {
                _output.MediaControls = new ParserOutput.MediaControlData()
                                            {
                                                IsEnabled = user.MediaControl.IsEnabled,
                                                AutoPause = user.MediaControl.AutoPause,
                                                FastForward = user.MediaControl.FastForward ?? -1,
                                                SecondsToRewind = user.MediaControl.SecondsToRewind,
                                                PlayPause = user.MediaControl.PlayPause ?? -1,
                                                Rewind = user.MediaControl.Rewind ?? -1,
                                                RewindToBeginning = user.MediaControl.RewindToBeginning ?? -1,
                                                Stop = user.MediaControl.Stop ?? -1
                                            };
            }

            _output.UserStyle = user.Style;


            _singleWords = input.Words.Item1;
            _words = input.Words.Item2;
        }
    }
}