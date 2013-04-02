using System;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public interface IGroupService
    {
        Group HasAccess(Guid groupId, Guid userId);
    }
}