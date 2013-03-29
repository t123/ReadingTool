using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using ReadingTool.Common;
using ReadingTool.Entities;

namespace ReadingTool.Services
{
    public class DefaultParserService : IParserService
    {
        protected bool _asParallel;
        protected Text _text;
        protected Language _l1Language;
        protected Language _l2Language;
        protected Term[] _singleTerms;
        protected Regex _termTest;
        protected Splitter _l1Splitter;
        protected Splitter _l2Splitter;

        public string Parse(bool asParallel, Language l1Language, Language l2Language, Term[] terms, Text text)
        {
            _asParallel = asParallel;
            _text = text;
            _l1Language = l1Language;
            _l2Language = l2Language;
            _singleTerms = terms;
            _l1Splitter = new Splitter(@"([^" + _l1Language.Settings.RegexWordCharacters + @"]+)", true);

            if(_l2Language != null)
            {
                _l2Splitter = new Splitter(@"([^" + _l2Language.Settings.RegexWordCharacters + @"]+)", true);
            }

            _termTest = new Regex(@"([" + _l1Language.Settings.RegexWordCharacters + @"])", RegexOptions.Compiled);

            string l1WithTitle = BuildTextWithTitle(_text.L1Text);
            string l2WithTitle = _asParallel ? BuildTextWithTitle(_text.L2Text) : string.Empty;

            var l1Split = SplitText(l1WithTitle, _l1Language.Settings);
            var l2Split = _l2Language == null ? string.Empty : SplitText(l2WithTitle, _l2Language.Settings);

            var xml = CreateTextXml(l1Split, l2Split);

            var result = ClassTerms(xml);
            xml = CreateStats(result.Item1, result.Item2);

            return ApplyTransform(xml);
        }

        protected virtual string BuildTextWithTitle(string inputText)
        {
            return (_text.CollectionNo.HasValue ? _text.CollectionNo + ". " : "") +
                   _text.Title +
                   (string.IsNullOrEmpty(_text.CollectionName) ? "" : " (" + _text.CollectionName + ")") +
                   ".\n\n" +
                   inputText;
        }

        protected virtual string SplitText(string text, Language.LanguageSettings settings)
        {
            if(string.IsNullOrEmpty(text) || settings == null) return string.Empty;

            text = text.Replace("\r\n", "\n").Replace("\n", "¶").Replace("\t", " ").Trim();
            text = Regex.Replace(text, @"\s{2,}/", " ");
            text = text.Replace("{", "[").Replace("}", "]");
            text = text.Trim();
            text = Regex.Replace(text, @"([" + (settings.RegexSplitSentences ?? string.Empty) + @"¶])\s", "$1\n");

            return text;
        }

        protected virtual XDocument CreateTextXml(string text, string parallelText)
        {
            var l1Settings = _l1Language.Settings;
            var l2Settings = _l2Language == null ? null : _l2Language.Settings;

            XDocument document = new XDocument();
            var rootNode = new XElement("root");
            rootNode.SetAttributeValue("title", _text.Title);

            var paragraphs = text.Split(new[] { '¶' }, StringSplitOptions.None);
            var parallelParagraphs = parallelText.Split(new[] { '¶' }, StringSplitOptions.None);

            for(int i = 0; i < paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i];
                var parallelParagraph = i < parallelParagraphs.Length ? parallelParagraphs[i] : "";

                var thisParagraph = CreateParagraph(paragraph, false, l1Settings, _l1Splitter);
                var thisParallelParagraph = CreateParagraph(parallelParagraph, true, l2Settings, _l2Splitter);

                if(!thisParagraph.HasElements && !thisParallelParagraph.HasElements) continue;

                var linkParagraphs = new XElement("link");
                thisParagraph.SetAttributeValue("side", "first");
                linkParagraphs.Add(thisParagraph);
                thisParallelParagraph.SetAttributeValue("side", "second");
                linkParagraphs.Add(thisParallelParagraph);
                rootNode.Add(linkParagraphs);
            }

            document.Add(rootNode);
            return document;
        }

        protected virtual XElement CreateParagraph(string paragraph, bool asParallel, Language.LanguageSettings settings, Splitter splitter)
        {
            var thisParagraph = new XElement("p");

            if(settings != null)
            {
                thisParagraph.SetAttributeValue("dir", settings.Direction.ToString().ToLowerInvariant());
            }
            else
            {
                thisParagraph.SetAttributeValue("dir", "ltr");
            }

            string[] sentences = paragraph.Split(new[] { "\n" }, StringSplitOptions.None);

            for(int i = 0; i < sentences.Length; i++)
            {
                string sentence = sentences[i];
                if(string.IsNullOrEmpty(sentence.TrimEnd()))
                {
                    thisParagraph.Add(new XElement("s"));
                    continue;
                }

                var thisSentence = new XElement("s");

                string[] tokens = splitter.Split(sentence);

                XElement t;
                foreach(var token in tokens)
                {
                    if(string.IsNullOrEmpty(token) || token == "\r" || token == "\n") continue;

                    if(token == " ")
                    {
                        //if(settings.ShowSpaces)
                        //{
                            t = new XElement("t");
                            t.SetAttributeValue("type", "space");
                            //t.SetAttributeValue("inSpan", asParallel ? "false" : "true");
                            t.SetAttributeValue("inSpan", true);
                        //}
                        //else
                        //{
                        //    continue;
                        //}
                    }
                    else if(_termTest.Match(token).Success)
                    {
                        t = new XElement("t");

                        if(asParallel)
                        {
                            t.SetAttributeValue("type", "parallel");
                        }
                        else
                        {
                            t.SetAttributeValue("type", "term");
                            string lower = token.ToLowerInvariant();
                            t.SetAttributeValue("lower", lower);
                            t.SetAttributeValue("state", Term.TermStateToClass(TermState.NotSeen));
                        }

                        t.SetAttributeValue("value", token);
                    }
                    else
                    {
                        t = new XElement("t");
                        t.SetAttributeValue("type", "punctuation");
                        t.SetAttributeValue("inSpan", asParallel ? "false" : "true");
                        t.SetAttributeValue("value", token);
                    }

                    thisSentence.Add(t);
                }

                t = new XElement("t");
                t.SetAttributeValue("type", "space");
                t.SetAttributeValue("inSpan", asParallel ? "false" : "true");
                thisSentence.Add(t);

                thisParagraph.Add(thisSentence);
            }

            return thisParagraph;
        }

        protected virtual XDocument CreateStats(int totalWords, XDocument document)
        {
            //var totalWords = document.Descendants("t").Count(x => x.Attribute("type").Value == "word");

            var toTake = (int)Math.Ceiling(totalWords / 1000f) * 20;

            var knownCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "term" && x.Attribute("state").Value == Term.TermStateToClass(TermState.Known));
            var unknownCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "term" && x.Attribute("state").Value == Term.TermStateToClass(TermState.NotKnown));
            var notseenCount = document.Descendants("t").Count(x => x.Attribute("type").Value == "term" && x.Attribute("state").Value == Term.TermStateToClass(TermState.NotSeen));
            var newWords = document
                .Descendants("t")
                .Where(x => x.Attribute("type").Value == "term" && x.Attribute("state").Value == Term.TermStateToClass(TermState.NotSeen))
                .Select(x => x.Attribute("value").Value)
                .GroupBy(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(y => new { Word = y, Count = y.Count() })
                .OrderByDescending(y => y.Count)
                .OrderBy(y => y.Word.ToString(), StringComparer.InvariantCultureIgnoreCase)
                .Take(toTake);

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

        protected virtual string ApplyTransform(XDocument document)
        {
            string xslText = _asParallel ? XsltResource.Read_Parallel : XsltResource.Read;

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

        protected virtual Tuple<int, XDocument> ClassTerms(XDocument document)
        {
            var totalTerms = document.Descendants("t").Count(x => x.Attribute("type").Value == "term");

            //Multilength
            foreach(var multi in _singleTerms.Where(x => x.Length > 1))
            {
                string lower = multi.Phrase.ToLowerInvariant();

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
                    node.Value = multi.Phrase;

                    if(multi.State == TermState.NotKnown)
                    {
                        node.SetAttributeValue("box", "box" + multi.Box);
                    }

                    node.SetAttributeValue("length", multi.Length);
                    node.SetAttributeValue("lower", lower);
                    node.SetAttributeValue("state", Term.TermStateToClass(multi.State));
                    node.SetAttributeValue("id", multi.TermId.ToString());
                    node.SetAttributeValue("data", multi.FullDefinition);

                    element.Add(node);
                }
            }

            var elements = document.Descendants("t").Where(x => x.Attribute("type").Value == "term");
            var termsAsDict = _singleTerms.Where(x => x.Length == 1).ToDictionary(x => x.Phrase.ToLowerInvariant(), x => new { State = x.State, FullDefinition = x.FullDefinition });

            foreach(var element in elements)
            {
                var lower = element.Attribute("lower").Value;

                if(termsAsDict.ContainsKey(lower))
                {
                    element.SetAttributeValue("state", Term.TermStateToClass(termsAsDict[lower].State));
                    element.SetAttributeValue("data", termsAsDict[lower].FullDefinition);
                    element.SetAttributeValue("new", "");
                }
                else
                {
                    element.SetAttributeValue("new", "_nw");
                }
            }

            return new Tuple<int, XDocument>(totalTerms, document);
        }
    }
}
