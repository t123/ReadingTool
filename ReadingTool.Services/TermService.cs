using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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
        void Delete(Term term);
        void Delete(Guid id);
        Term Find(Guid id);
        Term Find(Guid languageId, string term);
        IEnumerable<Term> FindAll();
        Tuple<Term[], Term[]> FindAllForParsing(Language language);
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

            foreach(var it in term.IndividualTerms)
            {
                Save(term.Id, it);
            }

            if(audit)
            {
                Logger.Info(new TermLog()
                    {
                        Date = DateTime.Now,
                        State = term.State,
                        TermId = term.Id,
                        LanguageId = term.LanguageId,
                        Owner = term.Owner,
                        IsNew = isNew,
                        StateChange = term.StateHasChanged
                    });
            }
        }

        private void Save(Guid termId, IndividualTerm individual)
        {
            if(individual.Id == Guid.Empty)
            {
                individual.TermId = termId;
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

        public Term Find(Guid id)
        {
            return _db.Select<Term>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();
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
        public Tuple<Term[], Term[]> FindAllForParsing(Language language)
        {
            var singleTerms = _db.Select<Term>(x => x.Length == 1 && x.LanguageId == language.Id && x.Owner == _identity.UserId);
            singleTerms.ForEach(x => x.AddIndividualTerms(_db.Select<IndividualTerm>(y => y.TermId == x.Id)));
            var multiTerms = _db.Select<Term>(x => x.Length > 1 && x.LanguageId == language.Id && x.Owner == _identity.UserId);
            multiTerms.ForEach(x => x.AddIndividualTerms(_db.Select<IndividualTerm>(y => y.TermId == x.Id)));

            return new Tuple<Term[], Term[]>(singleTerms.ToArray(), multiTerms.ToArray());
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

                            case @"definitions":
                                //TODO 
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
            const string columns = "l.Name, t.TermPhrase, t.Box, t.State, t.NextReview, t.Id, t.LanguageId";
            string sql = string.Format(@"
SELECT
/*ROWNUMBER*/
/*COLUMNS*/
FROM [Term] t
LEFT JOIN [Language] l ON t.LanguageId=l.Id
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
                var texts = _db.Query<Term>(query.ToString());
                var count = _db.Scalar<int>(countQuery);
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
