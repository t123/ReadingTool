using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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

        public DeleteService(MongoContext context, IPrincipal principal)
        {
            _context = context;
            _identity = principal.Identity as IUserIdentity;
        }

        public void DeleteUser(User user)
        {
            //if(user == null || user.Id != _identity.UserId)
            //{
            //    return;
            //}

            //_db.Select<Language>(x => x.Owner == user.Id).ForEach(DeleteLanguage);
            //_db.DeleteById<User>(user.Id);
            //var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Texts", user.Id.ToString());
            //DirectoryInfo di = new DirectoryInfo(directory);

            //if(di.Exists)
            //{
            //    di.Delete(true);
            //}
        }

        public void DeleteLanguage(Language language)
        {
            //if(language == null || language.Owner != _identity.UserId)
            //{
            //    return;
            //}

            //_db.Select<Term>(x => x.LanguageId == language.Id).ForEach(DeleteTerm);
            //_db.Select<Text>(x => x.L1Id == language.Id).ForEach(DeleteText);
            //_db.Update<Text>(new { L2Id = (Guid?)null }, x => x.L2Id == language.Id);
            //_db.DeleteById<Language>(language.Id);
        }

        public void DeleteTerm(Term term)
        {
            if(term == null || term.Owner != _identity.UserId)
            {
                return;
            }

            //_db.DeleteById<Term>(term.Id);
        }

        public void DeleteText(Text text)
        {
            if(text == null || text.Owner != _identity.UserId)
            {
                return;
            }

            //_db.Update<IndividualTerm>(new { TextId = (Guid?)null }, x => x.TextId == text.Id);
            //DeleteTextFile(text);
            //_db.DeleteById<Text>(text.Id);
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
