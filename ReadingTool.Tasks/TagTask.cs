#region License
// TagTask.cs is part of ReadingTool.Tasks
// 
// ReadingTool.Tasks is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Tasks is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Tasks. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Configuration;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class TagTask : DefaultTask
    {
        protected override void DoWork()
        {
            var collection = _db.GetCollection(Item.DbCollectionName);

            #region audio
            collection.Update(
                Query.And
                    (
                        Query.EQ("ItemType", 0),
                        Query.NE("Tags", TagHelper.Tags.AUDIO),
                        Query.NE("Url", "")
                    ),
                Update.Push("Tags", TagHelper.Tags.AUDIO).Pull("Tags", TagHelper.Tags.NO_AUDIO),
                UpdateFlags.Multi
                );

            collection.Update(
                Query.And
                    (
                        Query.EQ("ItemType", 0),
                        Query.NE("Tags", TagHelper.Tags.NO_AUDIO),
                        Query.EQ("Url", "")
                    ),
                Update.Push("Tags", TagHelper.Tags.NO_AUDIO).Pull("Tags", TagHelper.Tags.AUDIO),
                UpdateFlags.Multi
                );
            #endregion

            #region parallel
            collection.Update(
                Query.And
                    (
                        Query.EQ("ItemType", 0),
                        Query.EQ("Tags", TagHelper.Tags.PARALLEL)
                    ),
                Update.Pull("Tags", TagHelper.Tags.PARALLEL),
                UpdateFlags.Multi
                );

            collection.Update(
                Query.And
                    (
                        Query.EQ("ItemType", 0),
                        Query.NE("Tags", TagHelper.Tags.PARALLEL),
                        Query.EQ("IsParallel", true)
                    ),
                Update.Push("Tags", TagHelper.Tags.PARALLEL),
                UpdateFlags.Multi
                );
            #endregion

            #region newly created
            DateTime newCreated = DateTime.Now.AddDays(-30);

            collection.Update(
                Query.EQ("Tags", TagHelper.Tags.NEW),
                Update.Pull("Tags", TagHelper.Tags.NEW),
                UpdateFlags.Multi
                );

            collection.Update(
                Query.GTE("Created", newCreated),
                Update.Push("Tags", TagHelper.Tags.NEW),
                UpdateFlags.Multi
                );
            #endregion

            #region archived
            DateTime archiveDate = DateTime.Now.AddDays(-60);
            collection.Update(
                Query.EQ("Tags", TagHelper.Tags.ARCHIVE),
                Update.Pull("Tags", TagHelper.Tags.ARCHIVE),
                UpdateFlags.Multi
                );

            collection.Update(
                Query.And
                    (
                        Query.NE("Tags", TagHelper.Tags.ARCHIVE),
                        Query.LTE("LastSeen", archiveDate)
                    ),
                Update.Push("Tags", TagHelper.Tags.ARCHIVE),
                UpdateFlags.Multi
                );
            #endregion

            #region shared
            collection.Update(
                Query.EQ("Tags", TagHelper.Tags.SHARED),
                Update.Pull("Tags", TagHelper.Tags.SHARED),
                UpdateFlags.Multi
                );

            collection.Update(
                Query.And
                    (
                        Query.NE("Tags", TagHelper.Tags.SHARED),
                        Query.Not("GroupId").Size(0)
                    ),
                Update.Push("Tags", TagHelper.Tags.SHARED),
                UpdateFlags.Multi
                );
            #endregion
        }
    }
}
