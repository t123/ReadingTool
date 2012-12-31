using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core;
using ReadingTool.Core.Enums;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
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
    }

    public class TermService : ITermService
    {
        private readonly IDbConnection _db;
        private readonly IUserIdentity _identity;

        public TermService(IDbConnection db, IPrincipal principal)
        {
            _db = db;
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
                _db.Save(new TermLog()
                    {
                        Date = DateTime.Now,
                        State = term.State,
                        TermId = term.Id,
                        LanguageId = term.LanguageId,
                        Onwer = term.Owner,
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
