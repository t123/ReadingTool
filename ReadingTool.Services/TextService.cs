#region License
// TextService.cs is part of ReadingTool.Services
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
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.Text;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public class TextService : ITextService
    {
        private readonly Repository<Text> _textRepository;
        private readonly Repository<Language> _languageRepository;
        private readonly Repository<User> _userRepository;
        private readonly Repository<Group> _groupRepository;
        private readonly Repository<Term> _termRepository;
        private readonly IGroupService _groupService;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UserIdentity _identity;

        public TextService(
            Repository<Text> textRepository,
            Repository<Language> languageRepository,
            Repository<User> userRepository,
            Repository<Group> groupRepository,
            Repository<Term> termRepository,
            IGroupService groupService,
            IPrincipal principal
            )
        {
            _textRepository = textRepository;
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _termRepository = termRepository;
            _groupService = groupService;
            _identity = principal.Identity as UserIdentity;
        }

        public void Save(Text text)
        {
            if(text.Language2 == null)
            {
                text.L2Text = string.Empty;
            }

            if(text.TextId == Guid.Empty)
            {
                text.Created = DateTime.Now;
                text.TextId = SequentialGuid.NewGuid();
            }

            text.Modified = DateTime.Now;
            SaveTextContents(text);
            _textRepository.Save(text);
        }

        #region blob storage
        private string GetTextDirectory(Text text)
        {
            string path = UserDirectory.GetDirectory(text.User.UserId);

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private string GetTextName(Text text, bool l1)
        {
            string basePath = GetTextDirectory(text);

            if(l1)
            {
                return Path.Combine(basePath, string.Format("{0}.l1.zip", text.TextId.ToString()));
            }
            else
            {
                return Path.Combine(basePath, string.Format("{0}.l2.zip", text.TextId.ToString()));
            }
        }

        private Text ReadTextContents(Text text)
        {
            if(text == null)
            {
                return null;
            }

            string l1TextFilename = GetTextName(text, true);
            string l2TextFilename = GetTextName(text, false);

            if(File.Exists(l1TextFilename))
            {
                var contents = ReadFile(l1TextFilename);
                text.L1Text = Decompress(contents);
            }

            if(File.Exists(l2TextFilename))
            {
                var contents = ReadFile(l2TextFilename);
                text.L2Text = Decompress(contents);
            }

            return text;
        }

        private void SaveTextContents(Text text)
        {
            if(!string.IsNullOrEmpty(text.L1Text))
            {
                string filename = GetTextName(text, true);
                var bytes = Compress(text.L1Text);
                WriteFile(filename, bytes);
            }

            if(!string.IsNullOrEmpty(text.L2Text))
            {
                string filename = GetTextName(text, false);
                var bytes = Compress(text.L2Text);
                WriteFile(filename, bytes);
            }
        }

        private byte[] ReadFile(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        private void WriteFile(string filename, byte[] bytes)
        {
            File.WriteAllBytes(filename, bytes);
        }

        private byte[] Compress(string text)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(text ?? "");

            using(MemoryStream ms = new MemoryStream())
            {
                using(GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                    gzip.Close();
                }

                return ms.ToArray();
            }
        }

        private string Decompress(byte[] text)
        {
            using(GZipStream stream = new GZipStream(new MemoryStream(text), CompressionMode.Decompress))
            {
                const int size = 262144;
                byte[] buffer = new byte[size];
                using(MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if(count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while(count > 0);
                    return Encoding.UTF8.GetString(memory.ToArray());
                }
            }
        }
        #endregion

        public void Delete(Guid id)
        {
            var text = _textRepository.FindOne(id);

            if(text != null)
            {
                var query = _termRepository.Session.CreateQuery("update Term t set t.Text=null where t.Text.TextId=:textId").SetGuid("textId", id);
                query.ExecuteUpdate();

                query = _groupRepository.Session.CreateSQLQuery("delete from GroupText where TextId=:textId").SetGuid("textId", id);
                query.ExecuteUpdate();

                _textRepository.Delete(id);

                string l1TextFilename = GetTextName(text, true);
                string l2TextFilename = GetTextName(text, false);

                if(File.Exists(l1TextFilename))
                {
                    File.Delete(l1TextFilename);
                }

                if(File.Exists(l2TextFilename))
                {
                    File.Delete(l2TextFilename);
                }
            }
        }

        public Text FindOne(Guid id, Guid? groupId = null, Guid? userId = null)
        {
            Text text = null;

            if(groupId.HasValue && userId.HasValue)
            {
                var group = _groupService.HasAccess(groupId.Value, userId.Value);

                if(group != null)
                {
                    text = group.Texts.FirstOrDefault(x => x.TextId == id);
                }
            }
            else
            {
                text = _textRepository.FindOne(x => x.TextId == id && x.User.UserId == _identity.UserId);
            }

            if(text != null)
            {
                text = ReadTextContents(text);
            }

            return text;
        }

        public int Import(TextImport import)
        {
            var languages = _languageRepository.FindAll(x => x.User.UserId == _identity.UserId);
            var languageNameToLanguage = languages.ToDictionary(x => x.Name, x => x);

            var defaults = new TextImport.JsonDefaults
            {
                L1LanguageName = "",
                L2LanguageName = "",
                AutoNumberCollection = (bool?)null,
                CollectionName = "",
                StartCollectionWithNumber = (int?)null,
            };

            if(import.Defaults != null)
            {
                #region defaults for languages
                if(!string.IsNullOrEmpty(import.Defaults.L1LanguageName))
                {
                    if(!languageNameToLanguage.ContainsKey(import.Defaults.L1LanguageName))
                    {
                        throw new Exception(string.Format("The language name {0} in the defaults is not in your language list", import.Defaults.L1LanguageName));
                    }

                    defaults.L1LanguageName = import.Defaults.L1LanguageName;
                }

                if(!string.IsNullOrEmpty(import.Defaults.L2LanguageName))
                {
                    if(!languageNameToLanguage.ContainsKey(import.Defaults.L2LanguageName))
                    {
                        throw new Exception(string.Format("The language name {0} in the defaults is not in your language list", import.Defaults.L2LanguageName));
                    }

                    defaults.L2LanguageName = import.Defaults.L2LanguageName;
                }
                #endregion

                defaults.AutoNumberCollection = import.Defaults.AutoNumberCollection;
                defaults.CollectionName = import.Defaults.CollectionName;
                defaults.StartCollectionWithNumber = import.Defaults.StartCollectionWithNumber;
            }

            #region test texts and get the languageIds
            int counter = 1;
            StringBuilder errors = new StringBuilder();

            foreach(var text in import.Items)
            {
                #region get languages
                if(!string.IsNullOrEmpty(text.L1LanguageName) && !languageNameToLanguage.ContainsKey(text.L1LanguageName))
                {
                    errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items for item {1}<br/>", text.L1LanguageName, counter);
                }
                else if(string.IsNullOrEmpty(text.L1LanguageName) && string.IsNullOrEmpty(defaults.L1LanguageName))
                {
                    errors.AppendFormat("No language and no default language specified for item {0}<br/>", counter);
                }

                if(!string.IsNullOrEmpty(text.L2LanguageName) && !languageNameToLanguage.ContainsKey(text.L2LanguageName))
                {
                    errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items for item {1}<br/>", text.L2LanguageName, counter);
                }
                else if(string.IsNullOrEmpty(text.L2LanguageName) && string.IsNullOrEmpty(defaults.L2LanguageName) && !string.IsNullOrEmpty(text.L2Text))
                {
                    errors.AppendFormat("No (parallel) language and no default language specified for item {0}<br/>", counter);
                }

                #endregion

                if(string.IsNullOrEmpty(text.Title))
                {
                    errors.AppendFormat("The title was not specified for item {0}<br/>", counter);
                }

                if(string.IsNullOrEmpty(text.L1Text))
                {
                    errors.AppendFormat("The text was not specified for item {0}<br/>", counter);
                }

                counter++;
            }

            if(errors.Length > 0)
            {
                throw new Exception(errors.ToString());
            }
            #endregion

            #region import texts
            int imported = 0;
            int currentCollectionNo = (defaults.AutoNumberCollection ?? false)
                                            ? defaults.StartCollectionWithNumber ?? 1
                                            : 1;

            foreach(var text in import.Items)
            {
                var newText = new Text
                {
                    AudioUrl = text.AudioUrl,
                    CollectionName = string.IsNullOrEmpty(text.CollectionName) ? defaults.CollectionName : text.CollectionName,
                    User = _userRepository.LoadOne(_identity.UserId),
                    Language1 = languageNameToLanguage.GetValueOrDefault(text.L1LanguageName, languageNameToLanguage[defaults.L1LanguageName]),
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    Title = text.Title ?? "Import " + imported,
                };

                if(string.IsNullOrEmpty(text.L2LanguageName))
                {
                    if(string.IsNullOrEmpty(defaults.L2LanguageName))
                    {
                        if(!string.IsNullOrEmpty(newText.L2Text))
                        {
                            newText.Language2 = languageNameToLanguage.FirstOrDefault().Value;
                        }
                        else
                        {
                            newText.Language2 = null;
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(newText.L2Text))
                        {
                            newText.Language2 = languageNameToLanguage[defaults.L2LanguageName];
                        }
                        else
                        {
                            newText.Language2 = null;
                        }
                    }
                }
                else
                {
                    newText.Language2 = languageNameToLanguage[text.L2LanguageName];
                }

                if(text.CollectionNo != null)
                {
                    newText.CollectionNo = text.CollectionNo;
                }
                else
                {
                    if(defaults.AutoNumberCollection ?? false)
                    {
                        newText.CollectionNo = currentCollectionNo;
                        currentCollectionNo++;
                    }
                }

                Save(newText);

                imported++;
            }

            return imported;
            #endregion
        }
    }
}
