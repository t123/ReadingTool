#region License
// CleanUpTask.cs is part of ReadingTool.Tasks
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
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class CleanUpTask : ITask
    {
        private MongoDatabase _db;

        public SystemTaskResult Run(MongoDatabase db)
        {
            _db = db;
            Task task = null;
            try
            {
                task = Task.Factory.StartNew(CleanUp);
                task.Wait();
            }
            catch(Exception e)
            {
                return new SystemTaskResult() { Success = false, Message = e.Message, Exception = e };
            }

            if(!task.IsCompleted)
            {
                return new SystemTaskResult() { Success = false, Message = "Task did not complete, no exception" };
            }

            return new SystemTaskResult() { Success = true };
        }

        private void CleanUp()
        {
            _db.GetCollection(Token.CollectionName).Remove(Query.LT("Expiry", DateTime.Now));
            _db.GetCollection(ApiRequest.CollectionName).Remove(Query.LT("DateTime", DateTime.Now.AddHours(-24)));
            _db.GetCollection(Word.CollectionName).Remove(Query.EQ("WordPhrase", ""));
            _db.GetCollection(PasswordReset.CollectionName).Remove(Query.LT("Created", DateTime.Now.AddHours(-48)));
        }
    }
}
