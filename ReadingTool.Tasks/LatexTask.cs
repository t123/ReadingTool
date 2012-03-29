#region License
// LatexTask.cs is part of ReadingTool.Tasks
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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentMongo.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class LatexTask : DefaultTask
    {
        protected override void DoWork()
        {
            var values = _db.GetCollection<SystemSystemValues>("SystemSettings").FindOneById("default");

            var toParse =
                _db.GetCollection<LatexQueue>(LatexQueue.CollectionName)
                    .AsQueryable()
                    .Where(x => x.FileId == ObjectId.Empty)
                    .OrderBy(x => x.Created);

            var parser = _db.GetCollection<TextParser>(TextParser.CollectionName).FindOne(Query.EQ("Name", "Latex"));

            if(parser == null)
            {
                throw new NoNullAllowedException("Latex parser not found");
            }
            string body = @"Your file is now ready for download. You can download your file at [{0}{1}]({0}{1}).

Please note that:

* Your file is only downloadable by you.
* The link is only valid for 48 hours.

Thanks";

            string url = values.Site.Domain + @"texts/download/";
            foreach(var document in toParse)
            {
                var outfile = ParseText(parser, document.Latex);

                if(string.IsNullOrEmpty(outfile)) continue;

                MongoGridFS fs = new MongoGridFS(_db);
                var result = fs.Upload(outfile);
                document.FileId = result.Id.AsObjectId;
                _db.GetCollection(LatexQueue.CollectionName).Save(document);

                Message message = new Message()
                                      {
                                          Created = DateTime.Now,
                                          Body = string.Format(body, url, document.LatexQueueId),
                                          From = ObjectId.Empty,
                                          IsStarred = false,
                                          IsRead = false,
                                          MessageType = MessageType.Received,
                                          Owner = document.UserId,
                                          Subject = "Your PDF is ready",
                                          To = new List<ObjectId>() { document.UserId }
                                      };

                _db.GetCollection(Message.CollectionName).Save(message);
            }
        }

        private string ParseText(TextParser parser, string text)
        {
            return @"c:\temp\test.pdf";

            if(parser == null) return null;
            if(string.IsNullOrWhiteSpace(text)) return null;

            string infile = Path.GetTempFileName();
            string outfile = Path.GetTempFileName();

            using(TextWriter tw = new StreamWriter(infile))
            {
                tw.Write(text);
            }

            string cmd = parser.FullPath;
            string args = string.Format(parser.Arguments, infile, outfile);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = cmd;
            startInfo.Arguments = args;

            try
            {
                using(Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                return null;
            }

            return outfile;
        }
    }
}
