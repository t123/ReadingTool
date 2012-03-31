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
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void DoWork()
        {
            var values = _db.GetCollection<SystemSystemValues>("SystemSettings").FindOneById("default");

            var toParse =
                _db.GetCollection<LatexQueue>(LatexQueue.CollectionName)
                    .Find(Query.Exists("File", false))
                    .SetSortOrder("Created");

            var parser = _db.GetCollection<TextParser>(TextParser.CollectionName).FindOne(Query.EQ("Name", "Latex"));

            if(parser == null)
            {
                throw new NoNullAllowedException("Latex parser not found");
            }

            string body = @"Your file is now ready for download. You can download your file at [{0}{1}]({0}{1}).

Please note that:

* You must be logged in to access your file.
* Your file is only downloadable by you.
* Your file will be deleted in 24 hours. You may request a new PDF at any time.

Thanks";

            string url = values.Site.Domain + @"/texts/download/";
            foreach(var document in toParse)
            {
                Logger.Debug("Sending document to latex");
                var file = ParseText(parser, document.Latex);

                if(file == null)
                {
                    Logger.Debug("No data return from file");
                    continue;
                }

                document.File = file;
                _db.GetCollection(LatexQueue.CollectionName).Save(document);
                Logger.Debug("Saved file");

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
                Logger.Debug("Saved message");
            }
        }

        private byte[] ParseText(TextParser parser, string text)
        {
            if(parser == null) return null;
            if(string.IsNullOrWhiteSpace(text)) return null;

            string infile = Path.Combine(Path.GetTempFileName());
            string outfile = Path.ChangeExtension(infile, "pdf");
            string auxfile = Path.ChangeExtension(infile, "aux");
            string logfile = Path.ChangeExtension(infile, "log");

            try
            {
                Logger.DebugFormat("Infile: {0}", infile);
                Logger.DebugFormat("Outfile: {0}", outfile);

                using(TextWriter tw = new StreamWriter(infile))
                {
                    tw.Write(text);
                }

                string cmd = parser.FullPath;
                string args = string.Format(parser.Arguments, infile);

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = cmd;
                startInfo.Arguments = args;
                startInfo.WorkingDirectory = Path.GetTempPath();

                using(Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }

                Logger.DebugFormat("Reading outfile");
                byte[] buffer;
                using(FileStream fs = new FileStream(outfile, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                }

                return buffer;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                return null;
            }
            finally
            {
                if(File.Exists(infile))
                {
                    Logger.DebugFormat("Deleting infile");

                    try
                    {
                        File.Delete(infile);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e);
                    }
                }

                if(File.Exists(outfile))
                {
                    Logger.DebugFormat("Deleting outfile");

                    try
                    {
                        File.Delete(outfile);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e);
                    }
                }

                if(File.Exists(auxfile))
                {
                    Logger.DebugFormat("Deleting auxfile");

                    try
                    {
                        File.Delete(auxfile);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e);
                    }
                }

                if(File.Exists(logfile))
                {
                    Logger.DebugFormat("Deleting logfile");

                    try
                    {
                        File.Delete(logfile);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e);
                    }
                }
            }
        }
    }
}
