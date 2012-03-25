using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class DefaultTask : ITask
    {
        protected MongoDatabase _db;

        public SystemTaskResult Run(MongoDatabase db)
        {
            _db = db;
            Task task = null;
            try
            {
                task = Task.Factory.StartNew(DoWork);
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

        protected virtual void DoWork()
        {

        }
    }
}
