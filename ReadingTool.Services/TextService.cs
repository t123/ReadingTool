using System;
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
    public interface ITextService
    {
        void Save(Text text);
        void Delete(Text text);
        void Delete(Guid id);
        Text Find(Guid id);
        IEnumerable<Text> FindAll();
        Tuple<Guid?, Guid?> FindPagedTexts(Text text);
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
            if(text.Id == Guid.Empty)
            {
                text.Created = DateTime.Now;
                text.Owner = _identity.UserId;
                text.Id = SequentialGuid.NewGuid();
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

        private Guid? FindPreviousId(Text text)
        {
            return null;
        }

        private Guid? FindNextId(Text text)
        {
            return null;
        }
    }
}
