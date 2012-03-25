#region License
// AjaxController.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ReadingTool.Attributes;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Services;

namespace ReadingTool.Controllers.Ajax
{
    [CustomAuthorize]
    public class AjaxController : Controller
    {
        private const string OK = @"OK";
        private const string FAIL = @"FAIL";

        private readonly ISystemLanguageService _systemLanguageService;
        private readonly IWordService _wordService;
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IItemService _itemService;

        public AjaxController(
            ISystemLanguageService systemLanguageService,
            IWordService wordService,
            IUserService userService,
            IGroupService groupService,
            IItemService itemService
            )
        {
            _systemLanguageService = systemLanguageService;
            _wordService = wordService;
            _userService = userService;
            _groupService = groupService;
            _itemService = itemService;
        }

        public ActionResult Index()
        {
            return new HttpNotFoundResult();
        }

        #region autocompletes
        public JsonResult AutocompleteUsernames(string term)
        {
            var users = _userService.AutocompleteUsername(term);
            return Json(users.Select(x => x.Username).ToArray());
        }

        public JsonResult AutocompleteSystemLanguageName(string term)
        {
            var languages = _systemLanguageService.AutocompleteName(term);
            return Json(languages.Select(x => x.Name).ToArray());
        }

        public JsonResult AutocompleteTextCollectionName(string term)
        {
            var collectionNames = _itemService.AutocompleteCollectionName(ItemType.Text, term);
            return Json(collectionNames);
        }

        public JsonResult AutocompleteTextTags(string term)
        {
            var tags = _itemService.AutocompleteTags(ItemType.Text, term);
            return Json(tags);
        }

        public JsonResult AutocompleteGroupTags(string term)
        {
            var tags = _groupService.AutocompleteTags(term);
            return Json(tags.ToArray());
        }

        public JsonResult AutocompleteVideoCollectionName(string term)
        {
            var collectionNames = _itemService.AutocompleteCollectionName(ItemType.Video, term);
            return Json(collectionNames);
        }

        public JsonResult AutocompleteVideoTags(string term)
        {
            var tags = _itemService.AutocompleteTags(ItemType.Video, term);
            return Json(tags);
        }

        public JsonResult AutocompleteWordTags(string term)
        {
            var tags = _wordService.AutocompleteTags(term);
            return Json(tags.ToArray());
        }

        public JsonResult AutocompleteGroupNames(string term)
        {
            //TODO implement in service
            var names = _groupService
                .FindAllForUser(new[] { GroupMembershipType.Member, GroupMembershipType.Moderator, GroupMembershipType.Owner })
                .Where(x => x.Name.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Name);

            return Json(names.ToArray());
        }
        #endregion

        #region reading AJAX
        public JsonResult GetWord(string languageId, string word)
        {
            var result = _wordService.FindWordByLower(languageId, word);

            if(result == null)
            {
                return Json(null);
            }

            return Json(new
                            {
                                word = new
                                           {
                                               state = EnumHelper.GetAlternateName(result.State),
                                               baseWord = result.BaseWord,
                                               romanisation = result.Romanisation,
                                               definition = result.Definition,
                                               tags = string.Join(TagHelper.TAG_SEPARATOR, result.Tags ?? new string[] { }),
                                               sentence = result.Sentence,
                                               box = result.Box
                                           }
                            });
        }

        [ValidateInput(false)]
        public JsonResult SaveWord(
            string languageId,
            string word,
            string state,
            string baseWord,
            string romanisation,
            string definition,
            string tags,
            string sentence,
            string itemId
            )
        {
            try
            {
                var result = _wordService.SaveWord(languageId, word, state, baseWord, romanisation, definition, tags, sentence, itemId);
                List<string> data = new List<string>();
                if(!string.IsNullOrEmpty(result.BaseWord))
                {
                    data.Add(result.BaseWord);
                }

                return Json(
                    new
                    {
                        result = OK,
                        word = new
                        {
                            multiword = result.Length > 1,
                            wordLower = result.WordPhraseLower,
                            state = EnumHelper.GetAlternateName(result.State),
                            stateHuman = EnumHelper.GetDescription(result.State),
                            length = result.Length,
                            definition = result.FullDefinition
                        }
                    }
                );
            }
            catch
            {
                return Json(new { result = FAIL });
            }
        }

        [ValidateInput(false)]
        public JsonResult QuickSaveWord(
            string languageId,
            string word,
            string state,
            string itemId,
            string sentence
            )
        {
            try
            {
                var result = _wordService.SaveWord(languageId, word, state, itemId, sentence);
                return Json(
                    new
                    {
                        result = OK,
                        word = new
                                   {
                                       wordLower = result.WordPhraseLower,
                                       state = EnumHelper.GetAlternateName(result.State)
                                   }
                    }
                );
            }
            catch
            {
                return Json(new { result = FAIL });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult MarkRemainingAsKnown(string languageId, string[] words, string itemId)
        {
            if(
                words == null ||
                words.Length == 0 ||
                (words.Length == 1 && words[0].Equals(""))
                )
                return Json(new { result = OK });

            var result = _wordService.MarkRemainingAsKnown(languageId, words, itemId);
            return Json(new { result = result ? OK : FAIL });
        }
        #endregion
    }
}
