#region License
// ParserService.cs is part of ReadingTool.Services
// 
// ReadingTool.Services is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Services is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Services. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using MongoDB.Bson;
using MongoDB.Driver;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Exceptions;
using ReadingTool.Entities.Parser;
using StructureMap;

namespace ReadingTool.Services
{
    public class ParserService : BaseParserService
    {
        private readonly MongoDatabase _db;

        public ParserService(MongoDatabase db, IItemService itemService, SystemSystemValues values)
            : base(itemService, values)
        {
            _db = db;
        }

        public override ParserOutput Parse(ParserInput input)
        {
            DateTime start = DateTime.Now;
            Init(input);
            TimeSpan init = DateTime.Now - start;

            var timings = CreateOutput(input);

            #region keep track of parsing times
            try
            {
                BsonDocument document = new BsonDocument();
                document.Add(new BsonElement("Created", DateTime.Now));
                document.Add(new BsonElement("ItemId", input.Item.ItemId));
                document.Add(new BsonElement("UserId", input.User.UserId));
                document.Add(new BsonElement("MultiWords", _words.Length));
                document.Add(new BsonElement("SingleWords", _singleWords.Length));

                document.Add(new BsonElement("TotalWords", timings.Item1));
                document.Add(new BsonElement("Init", init.TotalSeconds));
                document.Add(new BsonElement("Tokenise", timings.Item2.TotalSeconds));
                document.Add(new BsonElement("Parse", timings.Item2.TotalSeconds));
                document.Add(new BsonElement("Class", timings.Item3.TotalSeconds));
                document.Add(new BsonElement("Stats", timings.Item4.TotalSeconds));
                document.Add(new BsonElement("Transform", timings.Item5.TotalSeconds));

                _db.GetCollection(Collections.ParsingTimes).Save(document);
            }
            catch
            {
                //suppress
            }
            #endregion

            return _output;
        }

        protected Tuple<int, TimeSpan, TimeSpan, TimeSpan, TimeSpan, TimeSpan> CreateOutput(ParserInput input)
        {
            input.Item.TokenisedText = string.Empty;

            DateTime start = DateTime.Now;
            if(string.IsNullOrEmpty(input.Item.TokenisedText))
            {
                var tokeniser = ObjectFactory.GetInstance<ITokeniserService>();
                var tokeniserOutput = tokeniser.Tokenise(input.Language, input.Item);
                input.Item = tokeniserOutput.Item;

                if(input.Item.ItemType == ItemType.Video)
                {
                    _output.VideoPlayback = tokeniserOutput.VideoPlaybackData;
                }
            }
            TimeSpan tokenise = DateTime.Now - start;

            start = DateTime.Now;
            var document = XDocument.Parse(input.Item.TokenisedText);
            TimeSpan parse = DateTime.Now - start;

            start = DateTime.Now;
            var classResult = ClassWords(document);
            var totalWords = classResult.Item1;
            document = classResult.Item2;
            TimeSpan classWords = DateTime.Now - start;

            start = DateTime.Now;
            document = CreateStats(totalWords, document);
            TimeSpan stats = DateTime.Now - start;

            start = DateTime.Now;
            _output.ParsedHtml = ApplyTransform(input, document);
            TimeSpan transform = DateTime.Now - start;

            return new Tuple<int, TimeSpan, TimeSpan, TimeSpan, TimeSpan, TimeSpan>(totalWords, tokenise, parse, classWords, stats, transform);
        }

        protected Tuple<int, XDocument> ClassWords(XDocument document)
        {
            var totalWords = document.Descendants("t").Count(x => x.Attribute("type").Value == "word");

            if(totalWords > _maxWords)
                throw new TooManyWords(_output.Item.ItemId, _output.Item.ItemType, totalWords, _maxWords);

            //Multilength
            foreach(var multi in _words)
            {
                var first = multi.WordPhraseLower.Split(' ').First();
                var partials = document
                    .Descendants("t")
                    .Where(x =>
                        x.Attribute("type").Value == "word" &&
                        x.Attribute("lower").Value.Equals(first, StringComparison.InvariantCultureIgnoreCase));

                if(partials == null || !partials.Any()) continue;

                foreach(var element in partials)
                {
                    var nodeString = element.Attribute("lower").Value + " " +
                        string.Join(
                            " ",
                            (element.ElementsAfterSelf().Where(x => x.Attribute("type").Value == "word").Take(multi.Length - 1))
                            .Select(x => x.Attribute("lower").Value)
                            );

                    if(!multi.WordPhraseLower.Equals(nodeString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var node = new XElement("multi");
                    node.Value = multi.WordPhrase;
                    node.SetAttributeValue("length", multi.Length);
                    node.SetAttributeValue("lower", multi.WordPhraseLower);
                    node.SetAttributeValue("state", _wordStates[multi.State]);
                    node.SetAttributeValue("id", multi.WordId.ToString());
                    node.SetAttributeValue("data", multi.FullDefinition);

                    element.Add(node);
                }
            }

            if(totalWords > _singleWords.Length)
            {
                foreach(var word in _singleWords)
                {
                    var elements = document.Descendants("t").Where(x => x.Attribute("type").Value == "word" && x.Attribute("lower").Value.Equals(word.WordPhraseLower));
                    foreach(var element in elements)
                    {
                        element.SetAttributeValue("state", _wordStates[word.State]);
                        element.SetAttributeValue("data", word.FullDefinition);
                    }
                }
            }
            else
            {
                var elements = document.Descendants("t").Where(x => x.Attribute("type").Value == "word");
                var wordsAsDict = _singleWords.ToDictionary(x => x.WordPhraseLower, x => new { State = x.State, FullDefinition = x.FullDefinition });

                foreach(var element in elements)
                {
                    var lower = element.Attribute("lower").Value;

                    if(wordsAsDict.ContainsKey(lower))
                    {
                        element.SetAttributeValue("state", _wordStates[wordsAsDict[lower].State]);
                        element.SetAttributeValue("data", wordsAsDict[lower].FullDefinition);
                    }
                }
            }

            return new Tuple<int, XDocument>(totalWords, document);
        }

        protected XDocument CreateStats(int totalWords, XDocument document)
        {
            //var totalWords = document.Descendants("t").Count(x => x.Attribute("type").Value == "word");
            var knownCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "word" && x.Attribute("state").Value == _wordStates[WordState.Known]);
            var unknownCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "word" && x.Attribute("state").Value == _wordStates[WordState.Unknown]);
            var notseenCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "word" && x.Attribute("state").Value == _wordStates[WordState.NotSeen]);
            var newWords = document
                .Descendants("t")
                .Where(x => x.Attribute("type").Value == "word" && x.Attribute("state").Value == _wordStates[WordState.NotSeen])
                .Select(x => x.Attribute("value").Value)
                .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(y => new { Word = y, Count = y.Count() })
                .OrderByDescending(y => y.Count)
                .OrderBy(y => y.Word.ToString(), StringComparer.InvariantCultureIgnoreCase)
                .Take(20);

            var tw = new XElement("totalWords", totalWords); tw.SetAttributeValue("percent", "100");
            var ns = new XElement("notseenCount", notseenCount);
            var kn = new XElement("knownCount", knownCount);
            var un = new XElement("unknownCount", unknownCount);

            if(int.Parse(tw.Value) > 0)
            {
                ns.SetAttributeValue("percent", string.Format("{0:0.00}", (double)ns / (double)tw * 100));
                kn.SetAttributeValue("percent", string.Format("{0:0.00}", (double)kn / (double)tw * 100));
                un.SetAttributeValue("percent", string.Format("{0:0.00}", (double)un / (double)tw * 100));
            }
            else
            {
                ns.SetAttributeValue("percent", 0);
                kn.SetAttributeValue("percent", 100);
                un.SetAttributeValue("percent", 0);
            }

            var data = new XElement("stats", tw, ns, kn, un,
                                    new XElement("unknownWords",
                                                 newWords.Select(x =>
                                                 {
                                                     var element = new XElement("word", x.Word.Key);
                                                     element.SetAttributeValue("count", x.Count);
                                                     return element;
                                                 }
                                                     ))
                );

            document.Descendants("root").First().Add(data);
            return document;
        }

        protected string ApplyTransform(ParserInput input, XDocument document)
        {
            var xslService = ObjectFactory.GetInstance<XslService>();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            StringBuilder sb = new StringBuilder();
            using(StringWriter sw = new StringWriter(sb))
            {
                using(XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    var xsl = xslService.XslForItem(input.Language.SystemLanguageId, input.Item.ItemType, input.AsParallel);
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(new StringReader(xsl)));
                    xslt.Transform(document.CreateReader(), writer);
                }
            }

            return sb.ToString();
        }
    }
}