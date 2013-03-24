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

            if(text.TextId == Guid.Empty)
            {
                text.Created = DateTime.Now;
            }

            text.Modified = DateTime.Now;
            _textRepository.Save(text);
        }

        public void Delete(Guid id)
        {
            var text = _textRepository.FindOne(id);

            if(text != null)
            {
                //TODO delete blob
                _textRepository.Delete(id);
            }
        }

        public Text FindOne(Guid id)
        {
            Text text = _textRepository.FindOne(x => x.TextId == id && x.User.UserId == _identity.UserId);

            //TODO Fetch texts from blob

            return text;
        }

        public int Import(TextImport import)
        {
            var languages = _languageRepository.FindAll();
            var languageNameToId = languages.ToDictionary(x => x.Name, x => x);
            //var ltrWithSpaces = _languageService.FindByName("Left to Right, with spaces", publicLanguage: true);

            dynamic defaults = new
            {
                Language1 = (Language)null,
                Language2 = (Language)null,
                AutoNumberCollection = (bool?)null,
                CollectionName = "",
                StartCollectionWithNumber = (int?)null,
            };

            if(import.Defaults != null)
            {
                #region defaults for languages
                if(!string.IsNullOrEmpty(import.Defaults.L1LanguageName))
                {
                    if(!languageNameToId.ContainsKey(import.Defaults.L1LanguageName))
                    {
                        throw new Exception(string.Format("The language name {0} in the defaults is not in your language list", import.Defaults.L1LanguageName));
                    }

                    defaults.Language1 = languageNameToId[import.Defaults.L1LanguageName];
                }

                if(!string.IsNullOrEmpty(import.Defaults.L2LanguageName))
                {
                    if(!languageNameToId.ContainsKey(import.Defaults.L2LanguageName))
                    {
                        throw new Exception(string.Format("The language name {0} in the defaults is not in your language list", import.Defaults.L2LanguageName));
                    }

                    defaults.Language2 = languageNameToId[import.Defaults.L2LanguageName];
                }
                #endregion

                defaults.AutoNumberCollection = defaults.Defaults.AutoNumberCollection;
                defaults.CollectionName = defaults.Defaults.CollectionName;
                defaults.StartCollectionWithNumber = defaults.Defaults.StartCollectionWithNumber;
            }

            #region test texts and get the languageIds
            int counter = 1;
            StringBuilder errors = new StringBuilder();

            foreach(var text in import.Items)
            {
                #region get languages
                if(!languageNameToId.ContainsKey(text.L1LanguageName ?? "") && defaults.Language1 == null)
                {
                    if(string.IsNullOrEmpty(text.L1LanguageName))
                    {
                        errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items, and no default was specified for item {1}<br/>", text.L1LanguageName ?? "none", counter);
                    }
                    else
                    {
                        errors.AppendFormat("No language or default was specified for item {0}<br/>", counter);
                    }
                }

                if(!string.IsNullOrEmpty(text.L2LanguageName) && !languageNameToId.ContainsKey(text.L2LanguageName))
                {
                    errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items, and no default was specified for item {1}<br/>", text.L2LanguageName, counter);
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
                    Language1 = languageNameToId.GetValueOrDefault(text.L1LanguageName, (Language)defaults.Language1),
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    Title = text.Title ?? "Import " + imported,
                };

                if(string.IsNullOrEmpty(text.L2LanguageName))
                {
                    if(defaults.Language2 == null)
                    {
                        if(!string.IsNullOrEmpty(newText.L2Text))
                        {
                            newText.Language2 = ((Language)defaults.Language2);
                        }
                        else
                        {
                            newText.Language2 = null;
                        }
                    }
                    else
                    {
                        newText.Language2 = ((Language)defaults.Language2);
                    }
                }
                else
                {
                    newText.Language2 = languageNameToId[text.L2LanguageName];
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
