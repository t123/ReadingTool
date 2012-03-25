#region License
// MecabTask.cs is part of ReadingTool.Tasks
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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;

namespace ReadingTool.Tasks
{
    public class MecabTask : DefaultTask
    {
        protected new void DoWork()
        {
            var items = _db.GetCollection<Item>(Item.DbCollectionName)
                .Find(Query.Exists("ParseWith", true))
                .SetSortOrder(SortBy.Ascending("Modified"));

            foreach(var item in items)
            {
                var parser = _db.GetCollection<TextParser>(TextParser.CollectionName)
                    .FindOne(Query.EQ("_id", item.ParseWith));

                if(parser != null)
                {
                    item.L1Text = ParseText(parser, item.L1Text);
                }

                item.ParseWith = ObjectId.Empty;
                _db.GetCollection<Item>(Item.DbCollectionName)
                    .Save(item);
            }
        }

        private string ParseText(TextParser parser, string text)
        {
            if(parser == null) return text;
            if(string.IsNullOrWhiteSpace(text)) return string.Empty;

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
                return text;
            }

            using(TextReader sr = new StreamReader(outfile))
            {
                string output = sr.ReadToEnd();
                return string.IsNullOrEmpty(output) ? text : output;
            }
        }
    }
}
