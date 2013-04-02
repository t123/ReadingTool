using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Repository;

namespace ReadingTool.Services
{
    public class GroupService : IGroupService
    {
        private readonly Repository<Group> _groupRepository;
        private readonly Repository<User> _userRepository;

        public GroupService(Repository<Group> groupRepository, Repository<User> userRepository)
        {
            _groupRepository = groupRepository;
            _userRepository = userRepository;
        }

        public bool HasAccess(Group group, User user)
        {
            return false;
        }

        public Group HasAccess(Guid groupId, Guid userId)
        {
            var group = _groupRepository.FindAll(
                x =>
                x.GroupId == groupId &&
                x.Members.Any(
                    y =>
                    y.User == _userRepository.LoadOne(userId) &&
                    (y.MembershipType == MembershipType.Member || y.MembershipType == MembershipType.Moderator || y.MembershipType == MembershipType.Owner)
                    )
                );

            return group.FirstOrDefault();
        }
    }
}
