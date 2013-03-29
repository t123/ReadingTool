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
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public class LatexParserService : DefaultParserService
    {
        public LatexParserService()
            : base()
        {
        }

        protected virtual Tuple<int, XDocument> ClassTerms(XDocument document)
        {
            var totalTerms = document.Descendants("t").Count(x => x.Attribute("type").Value == "term");

            //Multilength
            /*
            foreach(var multi in _terms)
            {
                string lower = multi.TermPhrase.ToLowerInvariant();

                var first = lower.Split(' ').First();
                var partials = document
                    .Descendants("t")
                    .Where(x =>
                        x.Attribute("type").Value == "term" &&
                        x.Attribute("lower").Value.Equals(first, StringComparison.InvariantCultureIgnoreCase));

                if(partials == null || !partials.Any()) continue;

                foreach(var element in partials)
                {
                    var nodeString = element.Attribute("lower").Value + " " +
                        string.Join(
                            " ",
                            (element.ElementsAfterSelf().Where(x => x.Attribute("type").Value == "term").Take(multi.Length - 1))
                            .Select(x => x.Attribute("lower").Value)
                            );

                    if(!lower.Equals(nodeString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var node = new XElement("multi");
                    node.Value = multi.TermPhrase;

                    if(multi.State == TermState.Unknown)
                    {
                        node.SetAttributeValue("box", "box" + multi.Box);
                    }

                    node.SetAttributeValue("length", multi.Length);
                    node.SetAttributeValue("lower", lower);
                    node.SetAttributeValue("state", Constants.TermStatesToClass[multi.State]);
                    node.SetAttributeValue("id", multi.Id.ToString());
                    node.SetAttributeValue("data", multi.Definition);

                    element.Add(node);
                }
            }
            */
            var elements = document.Descendants("t").Where(x => x.Attribute("type").Value == "term");
            var termsAsDict = _singleTerms.ToDictionary(x => x.Phrase.ToLowerInvariant(), x => new { State = x.State, FullDefinition = x.FullDefinition });

            foreach(var element in elements)
            {
                var lower = element.Attribute("lower").Value;

                if(termsAsDict.ContainsKey(lower))
                {
                    element.SetAttributeValue("state", Term.TermStateToClass(termsAsDict[lower].State));
                    element.SetAttributeValue("data", termsAsDict[lower].FullDefinition);
                }
            }

            return new Tuple<int, XDocument>(totalTerms, document);
        }

        protected override XDocument CreateStats(int totalWords, XDocument document)
        {
            return document;
        }

        protected override string ApplyTransform(XDocument document)
        {
            string xslText = XsltResource.Read_Latex;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            StringBuilder sb = new StringBuilder();
            using(StringWriter sw = new StringWriter(sb))
            {
                using(XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(new StringReader(xslText)));
                    xslt.Transform(document.CreateReader(), writer);
                }
            }

            return sb.ToString();
        }
    }
}