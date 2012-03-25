#region License
// GroupService.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Keys;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using Group = ReadingTool.Entities.Group;

namespace ReadingTool.Services
{
    public interface IGroupService
    {
        void Save(Group group);
        Group FindOne(string id);
        Group FindOne(ObjectId id);
        Group FindOneForUser(string id, GroupMembershipType[] types);
        Group FindOneForUser(ObjectId id, GroupMembershipType[] types);
        void Delete(string id);
        void Delete(ObjectId id);
        IEnumerable<Group> FindAll();
        IEnumerable<Group> FindAllForUser(GroupMembershipType[] types);
        IEnumerable<string> AutocompleteTags(string term);
        IEnumerable<string> AutocompleteTags(string term, int limit);

        Group FindOneByName(string name, ObjectId groupId);
        Group FindOneByName(string name);
        IEnumerable<GroupMember> FindAllMembers(ObjectId groupId, string[] folders);
        void AddGroupMember(GroupMember member);
        void AddGroupMember(IList<GroupMember> members);
        GroupMember GetMembership(ObjectId groupId);
        GroupMember GetMembership(ObjectId groupId, ObjectId userId);
        void Save(GroupMember member);
        //void DeleteMembership(ObjectId membershipId);
        void DeleteMembership(GroupMember member);


        MongoCursor<Group> SearchGroups(string[] folders, string filter, bool includePublic);

        IEnumerable<GroupMember> FindGroupMembership(IEnumerable<ObjectId> groupIds);
        IEnumerable<GroupMember> FindGroupMembership(IEnumerable<ObjectId> groupIds, ObjectId userId);
        int PendingRequestForGroup(ObjectId groupId);
        IEnumerable<Group> FindAll(IList<ObjectId> groupIds);
        void ChangeMembership(GroupMember membership, string s);
        void Save(GroupShareNotice groupShareNotice);

        IEnumerable<GroupShareNotice> FindAllForRss(string groupId);
        IEnumerable<GroupShareNotice> FindAllForRss(ObjectId groupId);
    }

    public class GroupService : IGroupService
    {
        private readonly MongoDatabase _db;
        private readonly UserForService _identity;

        public GroupService(
            MongoDatabase db,
            UserForService identity
            )
        {
            _db = db;
            _identity = identity;
        }

        public void Save(Group group)
        {
            GroupMember member = null;
            ObjectId id = ObjectId.Empty;

            if(group.GroupId == ObjectId.Empty)
            {
                id = ObjectId.GenerateNewId();

                group.GroupId = id;
                group.Created = DateTime.Now;
                group.Owner = _identity.UserId;
                member = new GroupMember { Created = DateTime.Now, Modified = DateTime.Now, Type = GroupMembershipType.Owner, UserId = _identity.UserId, GroupId = id };
            }

            group.Modified = DateTime.Now;
            _db.GetCollection(Collections.Groups).Save(group);

            if(member != null)
            {
                _db.GetCollection(Collections.GroupMembers).Save(member);
            }
        }

        public Group FindOne(string id)
        {
            return FindOne(new ObjectId(id));
        }

        public Group FindOne(ObjectId id)
        {
            return _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .FirstOrDefault(x => x.GroupId == id);
        }

        public void Delete(string id)
        {
            Delete(new ObjectId(id));
        }

        public void Delete(ObjectId id)
        {
            _db.GetCollection(Collections.Groups).Remove(
                Query.And(
                    Query.EQ("Owner", _identity.UserId),
                    Query.EQ("_id", id)
                    )
                );
        }

        public IEnumerable<Group> FindAll()
        {
            return _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .OrderBy(x => x.Name);
        }

        public IEnumerable<string> AutocompleteTags(string term)
        {
            return AutocompleteTags(term, 10);
        }

        public IEnumerable<string> AutocompleteTags(string term, int limit)
        {
            //TODO fixme to filter in DB
            var results =
                _db.GetCollection<Group>(Collections.Groups)
                .Distinct("Tags")
                .Select(x => x.AsString)
                .Where(x => x.StartsWith(term, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x);

            return limit < 0 ? results : results.Take(limit);
        }

        public IEnumerable<Group> FindAllForUser(GroupMembershipType[] types)
        {
            var groupIds = _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .Where(x => x.UserId == _identity.UserId && types.Contains(x.Type))
                .Select(x => x.GroupId);

            return _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .Where(x => groupIds.Contains(x.GroupId))
                .OrderBy(x => x.Name);
        }

        public Group FindOneForUser(string id, GroupMembershipType[] types)
        {
            return FindOneForUser(new ObjectId(id), types);
        }

        public Group FindOneForUser(ObjectId id, GroupMembershipType[] types)
        {
            var groupMember = _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .FirstOrDefault(x => x.UserId == _identity.UserId && types.Contains(x.Type) && x.GroupId == id);

            if(groupMember == null)
                return null;

            return _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .FirstOrDefault(x => x.GroupId == groupMember.GroupId);
        }

        public Group FindOneByName(string name)
        {
            return FindOneByName(name, ObjectId.Empty);
        }

        public Group FindOneByName(string name, ObjectId groupId)
        {
            string lower = (name ?? "").ToLowerInvariant().Trim();

            if(groupId == ObjectId.Empty)
            {
                return _db.GetCollection<Group>(Collections.Groups)
                    .AsQueryable()
                    .FirstOrDefault(x => x.NameLower == lower);
            }
            else
            {
                return _db.GetCollection<Group>(Collections.Groups)
                    .AsQueryable()
                    .FirstOrDefault(x => x.NameLower == name && x.GroupId != groupId);
            }
        }


        public IEnumerable<GroupMember> FindAllMembers(ObjectId groupId, string[] folders)
        {
            if(folders.Length == 1 && folders[0] == "")
            {
                return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                    .AsQueryable()
                    .Where(x => x.GroupId == groupId)
                    .OrderBy(x => x.Type);
            }

            var types = folders.Select(x => Enum.Parse(typeof(GroupMembershipType), x, true)).ToArray();

            return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .Find(
                Query.And(
                Query.EQ("GroupId", groupId),
                Query.In("Type", new BsonArray(types))
                )
                );
        }

        public void AddGroupMember(GroupMember member)
        {
            if(member == null)
                return;

            AddGroupMember(new List<GroupMember>() { member });
        }

        public void AddGroupMember(IList<GroupMember> members)
        {
            foreach(var member in members)
            {
                member.Modified = member.Created = DateTime.Now;
            }

            _db.GetCollection(Collections.GroupMembers).InsertBatch(members);
        }

        public GroupMember GetMembership(ObjectId groupId)
        {
            return GetMembership(groupId, _identity.UserId);
        }

        public GroupMember GetMembership(ObjectId groupId, ObjectId userId)
        {
            return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .FirstOrDefault(x => x.GroupId == groupId && x.UserId == userId);
        }

        public void Save(GroupMember member)
        {
            if(member.GroupMemberId == ObjectId.Empty)
            {
                member.Created = DateTime.Now;
                member.Modified = DateTime.Now;
            }

            _db.GetCollection(Collections.GroupMembers).Save(member);
        }

        public void DeleteMembership(GroupMember membership)
        {
            if(membership == null) return;
            if(membership.Type == GroupMembershipType.Owner) return;

            var membershipCollection = _db.GetCollection<GroupMember>(Collections.GroupMembers);

            if(membership != null)
            {
                RemoveGroupFromItems(membership.UserId, membership.GroupId);
            }

            membershipCollection.Remove(Query.EQ("_id", membership.GroupMemberId));
        }

        public MongoCursor<Group> SearchGroups(string[] folders, string filter, bool includePublic)
        {
            var types = new List<GroupMembershipType>();

            //Nothing chosen, default your groups
            bool nothing = false;
            if(folders.Length == 1 && folders[0] == "")
            {
                types.Add(GroupMembershipType.Member);
                types.Add(GroupMembershipType.Moderator);
                types.Add(GroupMembershipType.Owner);
                types.Add(GroupMembershipType.Invitation);
                nothing = true;
            }
            else
            {
                foreach(var folder in folders)
                {
                    if(folder == FilterKeys.Group.YOUR_GROUPS)
                    {
                        types.Add(GroupMembershipType.Member);
                        types.Add(GroupMembershipType.Moderator);
                        types.Add(GroupMembershipType.Owner);
                    }
                    else if(folder == FilterKeys.Group.MANAGE)
                    {
                        types.Add(GroupMembershipType.Moderator);
                        types.Add(GroupMembershipType.Owner);
                    }
                    else if(folder == FilterKeys.Group.INVITATIONS)
                    {
                        types.Add(GroupMembershipType.Invitation);
                    }
                }
            }

            types = types.Distinct().ToList();
            var groupIds = _db.GetCollection<GroupMember>(Collections.GroupMembers)
                        .AsQueryable()
                        .Where(x => x.UserId == _identity.UserId && types.Contains(x.Type))
                        .Select(x => x.GroupId);

            IEnumerable<ObjectId> publicIds = null;

            if(Array.IndexOf(folders, FilterKeys.Group.PUBLIC_GROUPS) >= 0 || (includePublic && nothing))
            {
                publicIds =
                        _db.GetCollection<Group>(Collections.Groups)
                        .AsQueryable()
                        .Where(x => x.Type == GroupType.Public)
                        .Select(x => x.GroupId);
            }

            var queries = new List<IMongoQuery>();

            if(publicIds == null)
            {
                queries.Add(Query.In("_id", new BsonArray(groupIds)));
            }
            else
            {
                queries.Add(
                    Query.Or(
                        Query.In("_id", new BsonArray(groupIds)),
                        Query.In("_id", new BsonArray(publicIds))
                    )
                    );
            }

            filter = (filter ?? "").ToLowerInvariant();
            string[] splitFilter = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] tags = splitFilter.Where(x => x.StartsWith("#")).Select(x => x.Substring(1, x.Length - 1)).ToArray();
            filter = string.Join(" ", splitFilter.Where(x => !x.StartsWith("#")));

            if(tags.Length > 0)
                queries.Add(Query.In("Tags", new BsonArray(tags)));

            if(!string.IsNullOrEmpty(filter))
            {
                queries.Add(Query.Matches("NameLower", BsonRegularExpression.Create(new Regex(filter))));
            }

            return _db.GetCollection<Group>(Collections.Groups).Find(Query.And(queries.ToArray())).SetSortOrder(SortBy.Ascending("Name"));
        }

        public IEnumerable<GroupMember> FindGroupMembership(IEnumerable<ObjectId> groupIds)
        {
            return FindGroupMembership(groupIds, _identity.UserId);
        }

        public IEnumerable<GroupMember> FindGroupMembership(IEnumerable<ObjectId> groupIds, ObjectId userId)
        {
            return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .Where(x => groupIds.Contains(x.GroupId) && x.UserId == userId);
        }

        public int PendingRequestForGroup(ObjectId groupId)
        {
            return _db.GetCollection<GroupMember>(Collections.GroupMembers)
                .AsQueryable()
                .Count(x => x.GroupId == groupId && x.Type == GroupMembershipType.Request);
        }

        public IEnumerable<Group> FindAll(IList<ObjectId> groupIds)
        {
            if(groupIds == null)
                return new List<Group>();

            return _db.GetCollection<Group>(Collections.Groups)
                .AsQueryable()
                .Where(x => groupIds.Contains(x.GroupId))
                .OrderBy(x => x.NameLower);
        }

        private void RemoveGroupFromItems(ObjectId userId, ObjectId groupId)
        {
            _db.GetCollection(Collections.Items)
                .Update
                (
                    Query.EQ("Owner", userId),
                    Update.Pull("GroupId", groupId),
                    UpdateFlags.Multi
                );
        }

        public void ChangeMembership(GroupMember member, string key)
        {
            if(member == null || member.Type == GroupMembershipType.Owner)
            {
                return;
            }

            switch(key)
            {
                case "moderator":
                    if(member.Type != GroupMembershipType.Invitation)
                        member.Type = GroupMembershipType.Moderator;
                    break;

                case "member":
                    if(member.Type != GroupMembershipType.Invitation)
                        member.Type = GroupMembershipType.Member;
                    break;

                case "banned":
                    if(member.Type != GroupMembershipType.Invitation)
                    {
                        member.Type = GroupMembershipType.Banned;
                        RemoveGroupFromItems(member.UserId, member.GroupId);
                    }
                    break;

                case "remove":
                    DeleteMembership(member);
                    return;

                case "nothing":
                    return;
                default:
                    return;
            }
        }

        public void Save(GroupShareNotice groupShareNotice)
        {
            if(groupShareNotice == null) return;
            groupShareNotice.Created = DateTime.Now;
            _db.GetCollection(Collections.GroupShareNotices).Save(groupShareNotice);
        }

        public IEnumerable<GroupShareNotice> FindAllForRss(string groupId)
        {
            return FindAllForRss(new ObjectId(groupId));
        }

        public IEnumerable<GroupShareNotice> FindAllForRss(ObjectId groupId)
        {
            return _db.GetCollection<GroupShareNotice>(Collections.GroupShareNotices)
                .AsQueryable()
                .Where(x => x.GroupId == groupId && x.ShareDirection == ShareDirection.Share)
                .OrderByDescending(x => x.Created)
                .Take(30);
        }
    }
}