using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Core.Database;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IDeleteService
    {
        void DeleteUser(User user);
        void DeleteLanguage(Language language);
        void DeleteText(Text text);
        void DeleteTerm(Term term);
    }

    public class DeleteService : IDeleteService
    {
        private readonly MongoContext _context;
        //TODO change to DELETE * WHERE x=x <-- allow IEnumerables

        private readonly IUserIdentity _identity;
        private readonly MongoCollection _userCollection;
        private readonly MongoCollection _textCollection;
        private readonly MongoCollection _languageCollection;
        private readonly MongoCollection _termCollection;

        public DeleteService(MongoContext context, IPrincipal principal)
        {
            _context = context;
            _identity = principal.Identity as IUserIdentity;

            _userCollection = _context.Database.GetCollection(typeof(User).Name);
            _languageCollection = _context.Database.GetCollection(typeof(Language).Name);
            _textCollection = _context.Database.GetCollection(typeof(Text).Name);
            _termCollection = _context.Database.GetCollection(typeof(Term).Name);
        }

        public void DeleteUser(User user)
        {
            if(user == null || user.Id != _identity.UserId)
            {
                return;
            }


            _languageCollection.Remove(Query.EQ("Owner", _identity.UserId));
            _termCollection.Remove(Query.EQ("Owner", _identity.UserId));
            _textCollection.Remove(Query.EQ("Owner", _identity.UserId));
            _userCollection.Remove(Query.EQ("_id", user.Id));
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Texts", user.Id.ToString());
            DirectoryInfo di = new DirectoryInfo(directory);

            if(di.Exists)
            {
                di.Delete(true);
            }
        }

        public void DeleteLanguage(Language language)
        {
            if(language == null || language.Owner != _identity.UserId)
            {
                return;
            }

            _termCollection.Remove(Query.EQ("LanguageId", language.Id));
            _textCollection.Remove(Query.EQ("L1Id", language.Id));
            _textCollection.Update(Query.EQ("L2Id", language.Id), Update.Set("L2Id", BsonNull.Value));
            _languageCollection.Remove(Query.EQ("_id", language.Id));
        }

        public void DeleteTerm(Term term)
        {
            if(term == null || term.Owner != _identity.UserId)
            {
                return;
            }

            _termCollection.Remove(Query.EQ("_id", term.Id));
        }

        public void DeleteText(Text text)
        {
            if(text == null || text.Owner != _identity.UserId)
            {
                return;
            }


            _textCollection.Remove(Query.EQ("_id", text.Id));
            _termCollection.Update(Query.EQ("TextId", text.Id), Update.Set("TextId", BsonNull.Value));
            DeleteTextFile(text);
        }

        private void DeleteTextFile(Text text)
        {
            if(text == null)
            {
                return;
            }

            var textDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Texts", _identity.UserId.ToString());
            var textFile = Path.Combine(textDirectory, string.Format("{0}.zip", text.Id));

            if(File.Exists(textFile))
            {
                File.Delete(textFile);
            }
        }
    }
}
