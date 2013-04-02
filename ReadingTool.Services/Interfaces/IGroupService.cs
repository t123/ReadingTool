using System;
using ReadingTool.Common;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IGroupService
    {
        Group HasAccess(Guid groupId, Guid userId, MembershipType[] types = null);
    }
}