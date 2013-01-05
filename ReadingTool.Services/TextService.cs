using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using ReadingTool.Core;
using ReadingTool.Core.Comparers;
using ReadingTool.Core.Enums;
using ReadingTool.Core.FilterParser;
using ReadingTool.Entities;
using ReadingTool.Entities.Search;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ITextService
    {
        void Save(Text text, bool ignoreModificationTime = false);
        void Delete(Text text);
        void Delete(Guid id);
        Text Find(Guid id);
        IEnumerable<Text> FindAll(bool includeText = false);
        Tuple<Guid?, Guid?> FindPagedTexts(Text text);
        int Import(TextImport import);
        SearchResult<Text> FilterTexts(SearchOptions so = null);
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
            SaveTextFiles(text);

            var tags = TagHelper.Split(text.Tags);
            _db.Delete<Tag>(x => x.TextId == text.Id);
            _db.InsertAll<Tag>(tags.Select(x => new Tag() { TermId = null, TextId = text.Id, Value = x }));
        }

        private string GetTextsDirectory(Text text)
        {
            if(text == null)
            {
                throw new NoNullAllowedException("Text cannot be null");
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Texts", _identity.UserId.ToString());
        }

        private void SaveTextFiles(Text text)
        {
            var directory = new DirectoryInfo(GetTextsDirectory(text));

            if(!directory.Exists)
            {
                directory.Create();
            }

            var outputFile = Path.Combine(directory.FullName, string.Format("{0}.zip", text.Id));

            using(ZipFile zip = new ZipFile())
            {
                zip.AddEntry("l1.txt", text.L1Text, Encoding.UTF8);
                zip.AddEntry("l2.txt", text.L2Text, Encoding.UTF8);
                zip.Save(outputFile);
            }
        }

        private Text LoadTextFiles(Text text)
        {
            var directory = new DirectoryInfo(GetTextsDirectory(text));

            if(!directory.Exists)
            {
                return text;
            }

            var inputFile = Path.Combine(directory.FullName, string.Format("{0}.zip", text.Id));

            if(!File.Exists(inputFile))
            {
                return text;
            }

            using(var zip = ZipFile.Read(inputFile))
            {
                var l1 = zip["l1.txt"];
                var l2 = zip["l2.txt"];

                using(var sr = new StreamReader(l1.OpenReader(), Encoding.UTF8))
                {
                    text.L1Text = sr.ReadToEnd();
                }

                using(var sr = new StreamReader(l2.OpenReader(), Encoding.UTF8))
                {
                    text.L2Text = sr.ReadToEnd();
                }
            }

            return text;
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
            var text = _db.Select<Text>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();

            if(text == null)
            {
                return null;
            }

            text = LoadTextFiles(text);

            return text;
        }

        public IEnumerable<Text> FindAll(bool includeTexts = false)
        {
            var texts = _db.Select<Text>(x => x.Owner == _identity.UserId);
            if(includeTexts)
            {
                //TODO fixme 
                return texts.Select(text => LoadTextFiles(text)).ToList();
            }
            else
            {
                return texts;
            }
        }

        public SearchResult<Text> FilterTexts(SearchOptions so = null)
        {
            if(so == null)
            {
                so = new SearchOptions();
            }

            #region ordering
            string orderBy;
            switch(so.Sort)
            {
                case "title":
                    orderBy = string.Format("ORDER BY t.Title {0}, l.Name ASC, t.CollectionName, t.CollectionNo, t.Title", so.Direction.ToString());
                    break;

                case "lastseen":
                    orderBy = string.Format("ORDER BY t.LastSeen {0}, l.Name ASC, t.CollectionName, t.CollectionNo, t.Title", so.Direction.ToString());
                    break;

                case "collectionname":
                    orderBy = string.Format("ORDER BY t.CollectionName ASC, t.CollectionNo, t.Title, l.Name", so.Direction.ToString());
                    break;

                case "language":
                default:
                    orderBy = string.Format("ORDER BY l.Name {0}, t.CollectionName, t.CollectionNo, t.Title", so.Direction.ToString());
                    break;
            }
            #endregion

            #region create where clause
            StringBuilder whereSql = new StringBuilder();

            var options = FilterParser.Parse(_languageService.FindAll().Select(x => x.Name.ToLowerInvariant()), so.Filter, FilterParser.MagicTextTags);

            #region magic
            if(options.Magic.Count > 0)
            {
                string magicSql = "";
                foreach(var o in options.Magic)
                {
                    switch(o)
                    {
                        case @"parallel":
                            magicSql += " AND t.IsParallel=1 ";
                            break;
                        case @"single":
                            magicSql += " AND t.IsParallel=0 ";
                            break;
                        case @"audio":
                            magicSql += " AND t.AudioURL IS NOT NULL AND LEN(t.AudioUrl)>0 ";
                            break;
                        case @"noaudio":
                            magicSql += " AND (t.AudioURL IS NULL OR LEN(t.AudioUrl)=0) ";
                            break;
                        case @"recent":
                            magicSql += " AND t.LastSeen>'" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            break;
                        case @"new":
                            magicSql += " AND t.Created>'" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            break;
                        case @"unseen":
                            magicSql += " AND t.LastSeen IS NULL ";
                            break;
                    }
                }

                whereSql.Append(magicSql);
            }
            #endregion

            #region languages
            if(options.Languages.Count > 0)
            {
                string languageSql = string.Format("AND l.Name IN ( {0} )", string.Join(",", options.Languages.Select(x => "'" + x + "'")));
                whereSql.Append(languageSql);
            }
            #endregion

            #region other
            if(options.Other.Count > 0)
            {
                string otherSql = string.Format("AND ( ");

                foreach(var o in options.Other)
                {
                    otherSql += string.Format("t.Title LIKE '%{0}%' OR t.CollectionName LIKE '%{0}%' OR ", o); //TODO escape
                }

                otherSql = otherSql.Substring(0, otherSql.Length - 4);
                otherSql += " )";
                whereSql.Append(otherSql);
            }
            #endregion

            #region tags
            if(options.Tags.Count > 0)
            {
                string tagSql = string.Format("AND T.Id IN ( SELECT TextId FROM Tag WHERE Tag.Value IN ({0}))", string.Join(",", options.Tags.Select(x => "'" + x + "'")));
                whereSql.Append(tagSql);
            }
            #endregion
            #endregion

            #region query creation
            const string columns = "l.Name, t.CollectionName, t.CollectionNo, t.Title, t.LastSeen, t.AudioUrl, t.Id, t.Tags, t.IsParallel, t.L1Id, t.Created, t.TimesRead, t.TimesListened, t.ListenLength, t.WordsRead";
            string sql = string.Format(@"
SELECT
/*ROWNUMBER*/
/*COLUMNS*/
FROM [Text] t
LEFT JOIN [Language] l ON t.L1Id=l.Id
WHERE t.Owner='{0}' /*WHERE*/
", _identity.UserId);

            string countQuery = sql.Replace("/*ROWNUMBER*/", "COUNT(t.Id) as Total").Replace("/*COLUMNS*/", "").Replace("/*WHERE*/", whereSql.ToString());

            StringBuilder query = new StringBuilder();
            query.AppendFormat(@"
SELECT *
FROM
(
{0}
) AS RowConstrainedResult
WHERE RowNumber BETWEEN {1} AND {2}
ORDER BY RowNumber
",
                         sql.Replace("/*ROWNUMBER*/", "ROW_NUMBER() OVER ( " + orderBy + " ) AS RowNumber,").Replace("/*COLUMNS*/", columns).Replace("/*WHERE*/", whereSql.ToString()),
                         (so.Page - 1) * so.RowsPerPage,
                         (so.Page - 1) * so.RowsPerPage + so.RowsPerPage
                );
            #endregion

            try
            {
                var texts = _db.Query<Text>(query.ToString());
                var count = _db.Scalar<int>(countQuery);
                return new SearchResult<Text> { Results = texts, TotalRows = count };
            }
            catch(Exception e)
            {
                var brokenSql = _db.GetLastSql();
                var message = string.Format("Invalid text search SQL:\n\n{0}\n\n{1}\n\n{2}", brokenSql, countQuery, query);
                throw new Exception(message, e);
            }
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
            var ltrWithSpaces = _languageService.FindByName("Left to Right, with spaces", publicLanguage: true);

            dynamic defaults = new
                {
                    L1Id = Guid.Empty,
                    L2Id = (Guid?)Guid.Empty,
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

                    defaults.L2Id = languageNameToId[import.Defaults.L2LanguageName];
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
                    Owner = _identity.UserId,
                    L1Id = languageNameToId.GetValueOrDefault(text.L1LanguageName, (Guid)defaults.L1Id),
                    //L2Id = languageNameToId.GetValueOrDefault(text.L2LanguageName, (Guid)defaults.L2Id),
                    L1Text = text.L1Text,
                    L2Text = text.L2Text,
                    Title = text.Title ?? "Import " + imported,
                    Tags = TagHelper.ToString(TagHelper.Merge(defaults.Tags, text.Tags)),
                };


                if(string.IsNullOrEmpty(text.L2LanguageName))
                {
                    if(defaults.L2Id == null || defaults.L2Id == Guid.Empty)
                    {
                        if(newText.IsParallel)
                        {
                            newText.L2Id = ltrWithSpaces.Id;
                        }
                        else
                        {
                            newText.L2Id = null;
                        }
                    }
                    else
                    {
                        newText.L2Id = ((Guid?)defaults.L2Id).Value;
                    }
                }
                else
                {
                    newText.L2Id = languageNameToId[text.L2LanguageName];
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

        private Guid? FindPreviousId(Text text)
        {
            var found = _db.Select<Text>("SELECT TOP 1 Id FROM [Text] WHERE Owner={0} AND L1Id={1} AND CollectionName={2} AND CollectionNo IS NOT NULL AND CollectionNo<{3} ORDER BY CollectionNo DESC",
                                         _identity.UserId,
                                         text.L1Id,
                                         text.CollectionName,
                                         text.CollectionNo
                ).FirstOrDefault();

            return found == null ? (Guid?)null : found.Id;
        }

        private Guid? FindNextId(Text text)
        {
            var found = _db.Select<Text>("SELECT TOP 1 Id FROM [Text] WHERE Owner={0} AND L1Id={1} AND CollectionName={2} AND CollectionNo IS NOT NULL AND CollectionNo>{3} ORDER BY CollectionNo ASC",
                                         _identity.UserId,
                                         text.L1Id,
                                         text.CollectionName,
                                         text.CollectionNo
                ).FirstOrDefault();

            return found == null ? (Guid?)null : found.Id;
        }
    }
}
