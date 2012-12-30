﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Core;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ITermService
    {
        void Save(Term term);
        void Delete(Term term);
        void Delete(Guid id);
        Term Find(Guid id);
        Term Find(Guid languageId, string term);
        IEnumerable<Term> FindAll();
        Tuple<Term[], Term[]> FindAllForParsing(Language language);
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
        public void Save(Term term)
        {
            if(term.Id == Guid.Empty)
            {
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
            throw new NotImplementedException();
        }

        public Term Find(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Term> FindAll()
        {
            throw new NotImplementedException();
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
