#region License
// LatexParserService.cs is part of ReadingTool.Services
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
using MongoDB.Driver.GridFS;
using ReadingTool.Common;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Exceptions;
using ReadingTool.Entities;
using ReadingTool.Entities.Parser;
using StructureMap;

namespace ReadingTool.Services
{
    public class LatexParserService : ParserService
    {
        public LatexParserService(MongoDatabase db, IItemService itemService, SystemSystemValues values)
            : base(db, itemService, values)
        {
        }

        public byte[] FindOne(ObjectId latexId, ObjectId userId)
        {
            var latex = _db.GetCollection<LatexQueue>(LatexQueue.CollectionName).FindOneById(latexId);

            if(latex == null || latex.UserId != userId)
            {
                return null;
            }

            return latex.File;
        }

        public void AddToQueue(ObjectId userId, ParserOutput output)
        {
            LatexQueue latex = new LatexQueue()
                                   {
                                       Created = DateTime.Now,
                                       UserId = userId,
                                       ItemId = new ObjectId(output.Item.ItemId),
                                       Latex = output.ParsedHtml,
                                   };

            _db.GetCollection(Collections.LatexQueue).Save(latex);
        }

        protected override Tuple<int, XDocument> ClassWords(XDocument document)
        {
            var totalWords = document.Descendants("t").Count(x => x.Attribute("type").Value == "word");

            if(totalWords > _maxWords)
                throw new TooManyWords(_output.Item.ItemId, _output.Item.ItemType, totalWords, _maxWords);

            var elements = document.Descendants("t").Where(x => x.Attribute("type").Value == "word");
            var wordsAsDict = _singleWords.ToDictionary(x => x.WordPhraseLower, x => new { State = x.State, FullDefinition = x.Definition });

            foreach(var element in elements)
            {
                var lower = element.Attribute("lower").Value;

                if(wordsAsDict.ContainsKey(lower))
                {
                    element.SetAttributeValue("state", _wordStates[wordsAsDict[lower].State]);
                    element.SetAttributeValue("data", wordsAsDict[lower].FullDefinition);
                }
            }

            return new Tuple<int, XDocument>(totalWords, document);
        }

        protected override XDocument CreateStats(int totalWords, XDocument document)
        {
            return document;
        }

        protected override string ApplyTransform(ParserInput input, XDocument document)
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
                    var xsl = xslService.FindOne("LatexRead");
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(new StringReader(xsl)));
                    xslt.Transform(document.CreateReader(), writer);
                }
            }

            return sb.ToString();
        }
    }
}