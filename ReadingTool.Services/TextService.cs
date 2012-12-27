using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Entities;
using ServiceStack.OrmLite;

namespace ReadingTool.Services
{
    public interface ITextService
    {
        void Save(Text text);
        void Delete(Text text);
        void Delete(long id);
        Text Find(long id);
        IEnumerable<Text> FindAll();
        Tuple<long?, long?> FindPagedTexts(Text text);
    }

    public class TextService : ITextService
    {
        private readonly IDbConnection _db;
        private readonly IUserIdentity _identity;

        public TextService(IDbConnection db, IPrincipal principal)
        {
            _db = db;
            _identity = principal.Identity as IUserIdentity;
        }

        public void Save(Text text)
        {
            if(text.Id == 0)
            {
                text.Created = DateTime.Now;
                text.Owner = _identity.UserId;
            }

            text.Modified = DateTime.Now;

            _db.Save(text);
        }

        public void Delete(Text text)
        {
            if(text == null)
            {
                return;
            }

            _db.DeleteById<Text>(text.Id);
        }

        public void Delete(long id)
        {
            Delete(Find(id));
        }

        public Text Find(long id)
        {
            return _db.Select<Text>(x => x.Id == id && x.Owner == _identity.UserId).FirstOrDefault();
        }

        public IEnumerable<Text> FindAll()
        {
            return _db.Select<Text>(x => x.Owner == _identity.UserId);
        }

        public Tuple<long?, long?> FindPagedTexts(Text text)
        {
            if(text == null) return new Tuple<long?, long?>(null, null);
            if(string.IsNullOrWhiteSpace(text.CollectionName)) return new Tuple<long?, long?>(null, null);
            if(!text.CollectionNo.HasValue) return new Tuple<long?, long?>(null, null);

            long? previousId = FindPreviousId(text);
            long? nextId = FindNextId(text);

            return new Tuple<long?, long?>(previousId, nextId);
        }

        private long? FindPreviousId(Text text)
        {
            return 1;
        }

        private long? FindNextId(Text text)
        {
            return 1;
        }
    }
}
