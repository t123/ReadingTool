using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UserIdentity _identity;

        public TextService(
            Repository<Text> textRepository,
            Repository<Language> languageRepository,
            Repository<User> userRepository,
            IPrincipal principal
            )
        {
            _textRepository = textRepository;
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _identity = principal.Identity as UserIdentity;
        }

        public void Save(Text text)
        {
            if(text.Language2 == null)
            {
                text.L2Text = string.Empty;
            }

            if(text.TextId == 0)
            {
                text.Created = DateTime.Now;
            }

            text.Modified = DateTime.Now;
            _textRepository.Save(text);
        }

        public void Delete(long id)
        {
            var text = _textRepository.FindOne(id);

            if(text != null)
            {
                //TODO delete blob
                _textRepository.Delete(id);
            }
        }

        public Text FindOne(long id)
        {
            Text text = _textRepository.FindOne(x => x.TextId == id && x.User.UserId == _identity.UserId);

            //TODO Fetch texts from blob

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
