using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using ReadingTool.Core;
using ReadingTool.Core.Database;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public interface ILanguageService : IRepository<Language>
    {
        IEnumerable<Language> FindAllIncludePublic();
        Language FindByName(string name, bool? publicLanguage = false);
        Tuple<List<TextStatResult>, List<TermStatResult>> FindBasicLanguageStats();
    }

    public class LanguageService : Repository<Language>, ILanguageService
    {
        private readonly IDeleteService _deleteService;
        private readonly IUserIdentity _identity;

        public LanguageService(MongoContext context, IPrincipal principal, IDeleteService deleteService)
            : base(context)
        {
            _deleteService = deleteService;
            _identity = principal.Identity as IUserIdentity;
        }

        public new void Save(Language language)
        {
            if(language.IsPublic && !_identity.IsInRole(Constants.Roles.ADMIN))
            {
                return;
            }

            if(language.Id == ObjectId.Empty)
            {
                language.Id = ObjectId.GenerateNewId();
                language.Owner = language.IsPublic ? User.DummyOwner : _identity.UserId;
            }

            language.Modified = DateTime.Now;

            base.Save(language);
        }

        public new void Delete(Language language)
        {
            if(language.IsPublic && !_identity.IsInRole(Constants.Roles.ADMIN))
            {
                return;
            }

            _deleteService.DeleteLanguage(language);
        }

        public new Language FindOne(ObjectId id)
        {
            var language = _collection.AsQueryable().FirstOrDefault(x => x.Id == id);

            if(language == null)
            {
                return null;
            }

            if(
                (language.Owner == _identity.UserId) ||
                (language.IsPublic && _identity.IsInRole(Constants.Roles.ADMIN))
                )
            {
                return language;
            }

            return null;
        }

        public new IEnumerable<Language> FindAll()
        {
            return _collection.FindAll().AsQueryable().Where(x => x.Owner == _identity.UserId);
        }

        public IEnumerable<Language> FindAllIncludePublic()
        {
            return _collection.FindAll().AsQueryable().Where(x => x.Owner == _identity.UserId || x.IsPublic);
        }

        public Language FindByName(string name, bool? publicLanguage = false)
        {
            if(publicLanguage == null)
            {
                return _collection.AsQueryable().FirstOrDefault(x => x.Name == name);
            }
            else
            {
                return _collection.AsQueryable().FirstOrDefault(x => x.Name == name && x.IsPublic == publicLanguage.Value);
            }
        }

        public Tuple<List<TextStatResult>, List<TermStatResult>> FindBasicLanguageStats()
        {
//            var textCount = @"
//SELECT l.Id, COUNT(t.Id) as Total, SUM(t.ListenLength) as ListenLength, SUM(t.WordsRead) as WordsRead
//FROM Language l
//LEFT JOIN Text t ON l.Id=t.L1Id
//WHERE l.Owner='{0}'
//GROUP BY l.Id
//";

//            var termCount = @"
//SELECT l.Id, t.State, COUNT(t.Id) as Total
//FROM Language l
//LEFT JOIN Term t ON l.Id=t.LanguageId
//WHERE l.Owner='{0}'
//GROUP BY l.Id, t.State
//";

//            var cmd = _db.CreateCommand();
//            cmd.CommandText = string.Format(textCount, _identity.UserId);
//            cmd.CommandType = CommandType.Text;

//            var texts = new List<TextStatResult>();
//            using(var reader = cmd.ExecuteReader())
//            {
//                while(reader.Read())
//                {
//                    texts.Add(new TextStatResult()
//                    {
//                        Id = reader.GetGuid(0),
//                        Count = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
//                        Listened = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
//                        Read = reader.IsDBNull(3) ? 0 : reader.GetInt64(3)
//                    });
//                }
//            }

//            cmd = _db.CreateCommand();
//            cmd.CommandText = string.Format(termCount, _identity.UserId);
//            cmd.CommandType = CommandType.Text;

//            var terms = new List<TermStatResult>();
//            using(var reader = cmd.ExecuteReader())
//            {
//                while(reader.Read())
//                {
//                    terms.Add(new TermStatResult()
//                        {
//                            Id = reader.GetGuid(0),
//                            State = reader.IsDBNull(1) ? "" : reader.GetString(1),
//                            Count = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
//                        });
//                }
//            }
            var texts = new List<TextStatResult>();
            var terms = new List<TermStatResult>();

            return new Tuple<List<TextStatResult>, List<TermStatResult>>(texts, terms);
        }
    }
}
