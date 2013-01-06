using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.FilterParser;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
using ReadingTool.Entities.Search;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ITermService
    {
        void Save(Term term, bool audit = true);
        void SaveAll(IEnumerable<Term> terms, bool audit = true);
        void Delete(Term term);
        void Delete(Guid id);
        Term Find(Guid id, bool includeIndividualTerms = false);
        Term Find(Guid languageId, string term);
        IEnumerable<Term> FindAll();
        Tuple<IList<Term>, IList<Term>> FindAllForParsing(Language language);
        IEnumerable<Term> FindAll(Guid languageId);
        Tuple<bool, string> ReviewTerm(Term term, Review review);
        SearchResult<Term> FilterTerms(SearchOptions searchOptions = null);
        IEnumerable<IndividualTerm> FindIndividualTerms(Guid termId);
        IEnumerable<IndividualTerm> FindIndividualTerms(Term term);
    }

    public class TermService : ITermService
    {
        private readonly IDbConnection _db;
        private readonly ILanguageService _languageService;
        private readonly IUserIdentity _identity;
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("TermAuditLog");

        public TermService(IDbConnection db, IPrincipal principal, ILanguageService languageService)
        {
            _db = db;
            _languageService = languageService;
            _identity = principal.Identity as IUserIdentity;
        }

        #region basic
        public void Save(Term term, bool audit = true)
        {
            bool isNew = false;
            if(term.Id == Guid.Empty)
            {
                isNew = true;
                term.Id = SequentialGuid.NewGuid();
                term.Owner = _identity.UserId;
            }

            term.TermPhrase = term.TermPhrase.Trim();
            term.Length = (short)term.TermPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            _db.Save(term);

            foreach(var it in term.DeletedIndividualTerms)
            {
                _db.Delete<IndividualTerm>(x => x.Id == it.Id);
            }

            foreach(var it in term.IndividualTerms)
            {
                Save(term.Id, term.LanguageId, it);
            }

            var language = _languageService.Find(term.LanguageId);

            if(audit)
            {
                var tl = new TermLog()
                    {
                        Date = DateTime.Now,
                        State = term.State,
                        TermId = term.Id,
                        LanguageId = term.LanguageId,
                        Owner = term.Owner,
                        IsNew = isNew,
                        StateChange = term.StateHasChanged,
                        //Language = language,
                        //Term = term
                    };

                Logger.Info(FormatAuditAsTSV(tl));
            }
        }

        private string FormatAuditAsTSV(TermLog log)
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                log.TermId, log.LanguageId, log.Owner, log.State, log.Date, log.IsNew, log.StateChange
                );
        }

        //private string FormatAuditAsJson(TermLog log)
        //{
        //    return JsonConvert.SerializeObject(log, new JsonSerializerSettings() { ContractResolver = new TermAuditSerializeContractResolver(), Formatting = Formatting.None });
        //}

        //public class TermAuditSerializeContractResolver : DefaultContractResolver
        //{
        //    public new static readonly TermAuditSerializeContractResolver Instance = new TermAuditSerializeContractResolver();

        //    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        //    {
        //        JsonProperty property = base.CreateProperty(member, memberSerialization);

        //        if(property.DeclaringType == typeof(Language) &&
        //            (property.PropertyName == "Settings") ||
        //            (property.PropertyName == "Review") ||
        //            (property.PropertyName == "Dictionaries")
        //            )
        //        {
        //            property.ShouldSerialize = instance => false;
        //        }
        //        else if(property.DeclaringType == typeof(Term) && property.PropertyName == "Definition")
        //        {
        //            property.ShouldSerialize = instance => false;
        //        }

        //        return property;
        //    }
        //}

        public void SaveAll(IEnumerable<Term> terms, bool audit = true)
        {
            StringBuilder sb = new StringBuilder();

            IList<IndividualTerm> its = new List<IndividualTerm>();
            var languages = _languageService.FindAll().ToDictionary(x => x.Id);

            foreach(var term in terms)
            {
                bool isNew = false;
                if(term.Id == Guid.Empty)
                {
                    term.Id = SequentialGuid.NewGuid();
                    term.Owner = _identity.UserId;
                    isNew = true;
                }

                term.TermPhrase = term.TermPhrase.Trim();
                term.Length = (short)term.TermPhrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

                foreach(var it in term.IndividualTerms)
                {
                    if(it.Id == Guid.Empty)
                    {
                        it.TermId = term.Id;
                        it.Id = SequentialGuid.NewGuid();
                        it.Created = DateTime.Now;
                        it.LanguageId = term.LanguageId;
                    }

                    it.Modified = DateTime.Now;
                    it.BaseTerm = it.BaseTerm.Trim();
                    it.Definition = it.Definition.Trim();
                    it.Sentence = it.Sentence.Trim();
                    it.Romanisation = it.Romanisation.Trim();
                    it.Tags = it.Tags.Trim().ToLowerInvariant();

                    if(!isNew)
                    {
                        _db.Delete<Tag>(x => x.TermId == it.Id);
                    }
                }

                its = its.Union(term.IndividualTerms).ToList();

                if(audit)
                {
                    var tl = new TermLog()
                    {
                        Date = DateTime.Now,
                        State = term.State,
                        TermId = term.Id,
                        LanguageId = term.LanguageId,
                        Owner = term.Owner,
                        IsNew = isNew,
                        StateChange = term.StateHasChanged,
                        //Language = languages[term.LanguageId],
                        //Term = term
                    };

                    sb.AppendLine(FormatAuditAsTSV(tl));
                }
            }

            _db.InsertAll(terms);
            _db.InsertAll(its);

            foreach(var it in its)
            {
                var tags = TagHelper.Split(it.Tags);
                _db.InsertAll<Tag>(tags.Select(x => new Tag() { TermId = it.Id, TextId = null, Value = x }));
            }

            if(audit)
            {
                Logger.Info(sb.ToString());
            }
        }

        private void Save(Guid termId, Guid languageId, IndividualTerm individual)
        {
            if(individual.Id == Guid.Empty)
            {
                individual.TermId = termId;
                individual.LanguageId = languageId;
                individual.Id = SequentialGuid.NewGuid();
                individual.Created = DateTime.Now;
            }

            individual.Modified = DateTime.Now;
            individual.BaseTerm = individual.BaseTerm.Trim();
            individual.Definition = individual.Definition.Trim();
            individual.Sentence = individual.Sentence.Trim();
            individual.Romanisation = individual.Romanisation.Trim();
            individual.Tags = individual.Tags.Trim().ToLowerInvariant();

            _db.Save(individual);

            var tags = TagHelper.Split(individual.Tags);
            _db.Delete<Tag>(x => x.TermId == individual.Id);
            _db.InsertAll<Tag>(tags.Select(x => new Tag() { TermId = individual.Id, TextId = null, Value = x }));
        }

        public void Delete(Term term)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            Delete(Find(id));
        }

        public Term Find(Guid id, bool includeIndividualTerms = false)
        {
            if(includeIndividualTerms)
            {
                var term = _db.Select<Term>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();

                if(term != null)
                {
                    term.AddIndividualTerms(FindIndividualTerms(term.Id));
                }

                return term;
            }
            else
            {
                return _db.Select<Term>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();
            }
        }

        public IEnumerable<Term> FindAll()
        {
            return _db.Select<Term>(x => x.Owner == _identity.UserId);
        }

        public IEnumerable<Term> FindAll(Guid languageId)
        {
            return _db.Select<Term>(x => x.LanguageId == languageId && x.Owner == _identity.UserId);
        }

        public bool Exists(Guid languageId, string termPhrase)
        {
            return _db.Select<Term>(x => x.LanguageId == languageId && x.TermPhrase == termPhrase).FirstOrDefault() != null;
        }
        #endregion

        #region parsing
        public Tuple<IList<Term>, IList<Term>> FindAllForParsing(Language language)
        {
            var allTerms = _db.Select<Term>(x => x.LanguageId == language.Id && x.Owner == _identity.UserId);
            var allIndividualTerm = _db.Select<IndividualTerm>(x => x.LanguageId == language.Id);

            var singleTerms = allTerms.Where(x => x.Length == 1).ToList();
            singleTerms.ForEach(x => x.AddIndividualTerms(allIndividualTerm.Where(z => z.TermId == x.Id)));

            var multiTerms = allTerms.Where(x => x.Length > 1).ToList();
            multiTerms.ForEach(x => x.AddIndividualTerms(allIndividualTerm.Where(z => z.TermId == x.Id)));

            //var singleTerms = _db.Select<Term>(x => x.Length == 1 && x.LanguageId == language.Id && x.Owner == _identity.UserId);
            //var singleIndividualTerm = _db.Select<IndividualTerm>(x => x.LanguageId == language.Id);
            //singleTerms.ForEach(x => x.AddIndividualTerms(singleIndividualTerm.Where(z => z.TermId == x.Id)));

            //var multiTerms = _db.Select<Term>(x => x.Length > 1 && x.LanguageId == language.Id && x.Owner == _identity.UserId);
            //var multiIndividualTerm = _db.Select<IndividualTerm>(x => x.LanguageId == language.Id);
            //multiTerms.ForEach(x => x.AddIndividualTerms(multiIndividualTerm.Where(z => z.TermId == x.Id)));

            return new Tuple<IList<Term>, IList<Term>>(singleTerms, multiTerms);
        }
        #endregion

        #region reading
        public Tuple<bool, string> ReviewTerm(Term term, Review review)
        {
            if(term == null) return new Tuple<bool, string>(false, string.Empty);
            if(term.State != TermState.Unknown) return new Tuple<bool, string>(false, string.Empty);
            if(term.NextReview.HasValue && DateTime.Now < term.NextReview) return new Tuple<bool, string>(false, string.Empty);

            if(review == null)
            {
                review = Review.Default;
            }

            term.Box++;

            if(term.Box > (review.KnownAfterBox ?? Review.MAX_BOXES))
            {
                term.State = TermState.Known;
            }
            else
            {
                term.NextReview = GetNextReview(review, term.Box);
            }

            Save(term);

            return new Tuple<bool, string>(true, string.Format("<strong>{0}<strong>: box {1}, due in {2}", term.TermPhrase, term.Box, (term.NextReview.Value - DateTime.Now).ToHumanAgo()));
        }

        public SearchResult<Term> FilterTerms(SearchOptions so = null)
        {
            if(so == null)
            {
                so = new SearchOptions();
            }

            #region ordering
            string orderBy;
            switch(so.Sort)
            {
                case "state":
                    orderBy = string.Format("ORDER BY t.State {0}, l.Name ASC, t.TermPhrase, t.State", so.Direction.ToString());
                    break;

                case "nextreview":
                    orderBy = string.Format("ORDER BY t.LastSeen {0}, l.Name ASC, t.TermPhrase, t.State", so.Direction.ToString());
                    break;

                case "termphrase":
                    orderBy = string.Format("ORDER BY t.TermPhrase {0},l.Name ASC, t.TermPhrase, t.State", so.Direction.ToString());
                    break;

                case "box":
                    orderBy = string.Format("ORDER BY t.Box {0}, l.Name ASC, t.TermPhrase, t.State", so.Direction.ToString());
                    break;

                case "language":
                default:
                    orderBy = string.Format("ORDER BY l.Name {0}, t.TermPhrase, t.State", so.Direction.ToString());
                    break;
            }
            #endregion

            #region create where clause
            StringBuilder whereSql = new StringBuilder();

            var options = FilterParser.Parse(_languageService.FindAll().Select(x => x.Name.ToLowerInvariant()), so.Filter, FilterParser.MagicTermTags);

            #region magic
            string having = "";
            if(options.Magic.Count > 0)
            {
                string magicSql = "";
                foreach(var o in options.Magic)
                {
                    if(o.StartsWith("box"))
                    {
                        magicSql += " AND t.Box=" + o.Substring(3, o.Length - 3) + " ";
                    }
                    else
                    {
                        switch(o)
                        {
                            //case @"new":
                            //    magicSql += " AND t.Created>'" + DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                            //    break;
                            case @"known":
                                magicSql += " AND t.State='" + TermState.Known.ToString() + "' ";
                                break;
                            case @"unknown":
                                magicSql += " AND t.State='" + TermState.Unknown.ToString() + "' ";
                                break;
                            case @"ignored":
                                magicSql += " AND t.State='" + TermState.Ignored.ToString() + "' ";
                                break;
                            case @"notseen":
                                magicSql += " AND t.State='" + TermState.NotSeen.ToString() + "' ";
                                break;

                            case @"nodefintions":
                                having = " HAVING COUNT(IT.Id)=0 ";
                                break;

                            case @"definitions":
                                having = " HAVING COUNT(IT.Id)>0 ";
                                break;
                        }
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
                    otherSql += string.Format("t.TermPhrase LIKE '%{0}%' OR ", o); //TODO escape
                }

                otherSql = otherSql.Substring(0, otherSql.Length - 4);
                otherSql += " )";
                whereSql.Append(otherSql);
            }
            #endregion

            #region tags
            if(options.Tags.Count > 0)
            {
                string tagSql = string.Format("AND T.Id IN ( SELECT TermId FROM IndividualTerm WHERE Id IN ( SELECT TermId FROM Tag WHERE Value IN ({0})))", string.Join(",", options.Tags.Select(x => "'" + x + "'")));
                whereSql.Append(tagSql);
            }
            #endregion
            #endregion

            #region query creation
            const string columns = "l.Name, t.TermPhrase, t.Box, t.State, t.NextReview, t.Id, t.LanguageId, COUNT(IT.Id) as ITCount";
            string sql = string.Format(@"
SELECT
/*ROWNUMBER*/
/*COLUMNS*/
FROM [Term] t
LEFT JOIN [Language] l ON t.LanguageId=l.Id 
/*LEFTJOINIT*/
WHERE t.Owner='{0}' /*WHERE*/
/*GROUPBY*/
/*HAVING*/
", _identity.UserId);

            string countQuery = sql
                .Replace("/*ROWNUMBER*/", "COUNT(t.Id) as Total")
                .Replace("/*COLUMNS*/", "")
                .Replace("/*HAVING*/", "")
                .Replace("/*GROUPBY*/", "")
                .Replace("/*LEFTJOINIT*/", "")
                .Replace("/*WHERE*/", whereSql.ToString());

            int page = so.Page - 1;
            int rowsPerPage = so.RowsPerPage;

            if(so.IgnorePaging)
            {
                page = 0;
                rowsPerPage = int.MaxValue;
            }

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
                         sql
                         .Replace("/*ROWNUMBER*/", "ROW_NUMBER() OVER ( " + orderBy + " ) AS RowNumber,")
                         .Replace("/*COLUMNS*/", columns)
                         .Replace("/*LEFTJOINIT*/", "LEFT JOIN [IndividualTerm] IT ON t.Id=it.TermId ")
                         .Replace("/*HAVING*/", having)
                         .Replace("/*GROUPBY*/", "GROUP BY l.Name, t.TermPhrase, t.Box, t.State, t.NextReview, t.Id, t.LanguageId")
                         .Replace("/*WHERE*/", whereSql.ToString()),
                         page * rowsPerPage,
                         page * rowsPerPage + rowsPerPage
                );
            #endregion

            try
            {
                var texts = _db.Query<Term>(query.ToString());
                var count = _db.Scalar<int>(countQuery); //TODO FIXME #definitions doesn't count correctly.
                return new SearchResult<Term> { Results = texts, TotalRows = count };
            }
            catch(Exception e)
            {
                var brokenSql = _db.GetLastSql();
                var message = string.Format("Invalid text search SQL:\n\n{0}\n\n{1}\n\n{2}", brokenSql, countQuery, query);
                throw new Exception(message, e);
            }
        }

        public IEnumerable<IndividualTerm> FindIndividualTerms(Guid termId)
        {
            return FindIndividualTerms(Find(termId));
        }

        public IEnumerable<IndividualTerm> FindIndividualTerms(Term term)
        {
            if(term == null)
            {
                return new IndividualTerm[0];
            }

            return _db.Where<IndividualTerm>("TermId", term.Id);
        }

        private DateTime GetNextReview(Review review, int? currentLevel)
        {
            if(review == null) review = Review.Default;

            if(currentLevel == null)
            {
                currentLevel = 1;
            }

            switch(currentLevel)
            {
                case 1:
                    return DateTime.Now.AddMinutes(review.Box1Minutes ?? Review.Default.Box1Minutes.Value);

                case 2:
                    return DateTime.Now.AddMinutes(review.Box2Minutes ?? Review.Default.Box2Minutes.Value);

                case 3:
                    return DateTime.Now.AddMinutes(review.Box3Minutes ?? Review.Default.Box3Minutes.Value);

                case 4:
                    return DateTime.Now.AddMinutes(review.Box4Minutes ?? Review.Default.Box4Minutes.Value);

                case 5:
                    return DateTime.Now.AddMinutes(review.Box5Minutes ?? Review.Default.Box5Minutes.Value);

                case 6:
                    return DateTime.Now.AddMinutes(review.Box6Minutes ?? Review.Default.Box6Minutes.Value);

                case 7:
                    return DateTime.Now.AddMinutes(review.Box7Minutes ?? Review.Default.Box7Minutes.Value);

                case 8:
                    return DateTime.Now.AddMinutes(review.Box8Minutes ?? Review.Default.Box8Minutes.Value);

                case 9:
                    return DateTime.Now.AddMinutes(review.Box9Minutes ?? Review.Default.Box9Minutes.Value);

                default:
                    return DateTime.Now.AddMinutes(review.Box9Minutes ?? Review.Default.Box9Minutes.Value);
            }
        }

        public Term Find(Guid languageId, string termPhrase)
        {
            Term term = _db.Select<Term>(x => x.TermPhrase == termPhrase && x.LanguageId == languageId && x.Owner == _identity.UserId).FirstOrDefault();

            if(term == null) return null;

            var individual = _db.Select<IndividualTerm>(x => x.TermId == term.Id);
            term.AddIndividualTerms(individual);

            return term;
        }

        #endregion
    }
}
