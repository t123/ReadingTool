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
        void Save(Text text, bool ignoreModificationTime = false);
        void Delete(Text text);
        void Delete(Guid id);
        Text Find(Guid id);
        IEnumerable<Text> FindAll();
        Tuple<Guid?, Guid?> FindPagedTexts(Text text);
    }

    public class TextService : ITextService
    {
        private readonly IDbConnection _db;
        private readonly IDeleteService _deleteService;
        private readonly IUserIdentity _identity;

        public TextService(IDbConnection db, IPrincipal principal, IDeleteService deleteService)
        {
            _db = db;
            _deleteService = deleteService;
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

            var tags = TagHelper.Split(text.Tags);
            _db.Delete<Tag>(x => x.TextId == text.Id);
            _db.InsertAll<Tag>(tags.Select(x => new Tag() { TermId = null, TextId = text.Id, Value = x }));
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
            //TODO implement me
            return null;
        }

        private Guid? FindNextId(Text text)
        {
            //TODO implement me
            return null;
        }
    }
}
