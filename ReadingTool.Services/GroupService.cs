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

        public Group HasAccess(Guid groupId, Guid userId, MembershipType[] types = null)
        {
            if(types == null)
            {
                types = new MembershipType[] { MembershipType.Member, MembershipType.Moderator, MembershipType.Owner };
            }

            var group = _groupRepository.FindAll(
                x =>
                x.GroupId == groupId &&
                x.Members.Any(
                    y =>
                    y.User == _userRepository.LoadOne(userId) &&
                    (types.Contains(y.MembershipType))
                    )
                );

            return group.FirstOrDefault();
        }
    }
}
