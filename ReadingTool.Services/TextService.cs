using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core;
using ReadingTool.Core.Comparers;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ITextService
    {
        void Save(Text text, bool ignoreModificationTime = false);
        void Delete(Text text);
        void Delete(Guid id);
        Text Find(Guid id);
        IEnumerable<Text> FindAll();
        Tuple<Guid?, Guid?> FindPagedTexts(Text text);
        int Import(TextImport import);
    }

    public class TextService : ITextService
    {
        private readonly IDbConnection _db;
        private readonly IDeleteService _deleteService;
        private readonly ILanguageService _languageService;
        private readonly IUserIdentity _identity;

        public TextService(IDbConnection db, IPrincipal principal, IDeleteService deleteService, ILanguageService languageService)
        {
            _db = db;
            _deleteService = deleteService;
            _languageService = languageService;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Save(Text text, bool ignoreModificationTime = false)
        {
            if(text.Id == Guid.Empty)
            {
                text.Created = DateTime.Now;
                text.Owner = _identity.UserId;
                text.Id = SequentialGuid.NewGuid();
            }

            if(!ignoreModificationTime)
            {
                text.Modified = DateTime.Now;
            }

            _db.Save(text);

            var tags = TagHelper.Split(text.Tags);
            _db.Delete<Tag>(x => x.TextId == text.Id);
            _db.InsertAll<Tag>(tags.Select(x => new Tag() { TermId = null, TextId = text.Id, Value = x }));
        }

        public void Delete(Text text)
        {
            _deleteService.DeleteText(text);
        }

        public void Delete(Guid id)
        {
            Delete(Find(id));
        }

        public Text Find(Guid id)
        {
            return _db.Select<Text>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();
        }

        public IEnumerable<Text> FindAll()
        {
            return _db.Select<Text>(x => x.Owner == _identity.UserId);
        }

        public Tuple<Guid?, Guid?> FindPagedTexts(Text text)
        {
            if(text == null) return new Tuple<Guid?, Guid?>(null, null);
            if(string.IsNullOrWhiteSpace(text.CollectionName)) return new Tuple<Guid?, Guid?>(null, null);
            if(!text.CollectionNo.HasValue) return new Tuple<Guid?, Guid?>(null, null);

            Guid? previousId = FindPreviousId(text);
            Guid? nextId = FindNextId(text);

            return new Tuple<Guid?, Guid?>(previousId, nextId);
        }


        public int Import(TextImport import)
        {
            var languages = _languageService.FindAll();
            var languageNameToId = languages.ToDictionary(x => x.Name.ToLowerInvariant(), x => x.Id, new CaseInsensitiveComparer());

            dynamic defaults = new
                {
                    L1Id = Guid.Empty,
                    L2Id = Guid.Empty,
                    AutoNumberCollection = (bool?)null,
                    CollectionName = "",
                    StartCollectionWithNumber = (int?)null,
                    Tags = ""
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

                    defaults.L1Id = languageNameToId[import.Defaults.L1LanguageName];
                }

                if(!string.IsNullOrEmpty(import.Defaults.L2LanguageName))
                {
                    if(!languageNameToId.ContainsKey(import.Defaults.L2LanguageName))
                    {
                        throw new Exception(string.Format("The language name {0} in the defaults is not in your language list", import.Defaults.L2LanguageName));
                    }

                    defaults.L1Id = languageNameToId[import.Defaults.L1LanguageName];
                }
                #endregion

                defaults.AutoNumberCollection = defaults.Defaults.AutoNumberCollection;
                defaults.CollectionName = defaults.Defaults.CollectionName;
                defaults.StartCollectionWithNumber = defaults.Defaults.StartCollectionWithNumber;
                defaults.Tags = defaults.Defaults.Tags;
            }

            #region test texts and get the languageIds
            int counter = 1;
            StringBuilder errors = new StringBuilder();

            foreach(var text in import.Items)
            {
                #region get languages
                if(!languageNameToId.ContainsKey(text.L1LanguageName ?? "") && defaults.L1Id == Guid.Empty)
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

                //text.L1Id = language == null ? Guid.Empty : language.Id;

                if(!languageNameToId.ContainsKey(text.L2LanguageName ?? "") && defaults.L1Id == Guid.Empty)
                {
                    if(string.IsNullOrEmpty(text.L2LanguageName))
                    {
                        errors.AppendFormat("The language '<b>{0}</b>' was not found in the text items, and no default was specified for item {1}<br/>", text.L2LanguageName ?? "none", counter);
                    }
                    else
                    {
                        errors.AppendFormat("No language or default was specified for item {0}<br/>", counter);
                    }
                }

                //text.L2Id = language == null ? Guid.Empty : language.Id;
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
                    Owner = _identity.UserId,
                    L1Id = languageNameToId.GetValueOrDefault(text.L1LanguageName, (Guid)defaults.LanguageId),
                    L2Id = languageNameToId.GetValueOrDefault(text.L2LanguageName, (Guid)defaults.LanguageId),
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    Title = text.Title ?? "Import " + imported,
                    Tags = TagHelper.Merge(defaults.Tags, text.Tags),
                };

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

        private Guid? FindPreviousId(Text text)
        {
            //TODO implement me
            return null;
        }

        private Guid? FindNextId(Text text)
        {
            //TODO implement me
            return null;
        }
    }
}
