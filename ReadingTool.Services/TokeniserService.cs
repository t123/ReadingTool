#region License
// TokeniserService.cs is part of ReadingTool.Services
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Exceptions;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Parser;

namespace ReadingTool.Services
{
    public interface ITokeniserService
    {
        ParserTokeniserDto Tokenise(Language language, Item item);
    }

    public class TokeniserService : ITokeniserService
    {
        protected Item _item;
        protected Splitter _sentenceSplitter;
        protected Splitter _tokenSplitter;
        protected Language _language;
        protected string _notseenState = EnumHelper.GetAlternateName(WordState.NotSeen);

        protected class ItemTokeniserOutput
        {
            public XDocument Document { get; set; }
            public IList<ParserOutput.VideoPlaybackData> PlaybackData { get; set; }
        }

        public TokeniserService()
        {
        }

        public ParserTokeniserDto Tokenise(Language language, Item item)
        {
            _language = language;
            _sentenceSplitter = new Splitter(_language.SentenceEndRegEx, true);
            _tokenSplitter = new Splitter(_language.ParsingPunctuationExpression);
            _item = item;

            if(_item.ItemType == ItemType.Video)
            {
                var vo = CreateVideoXml(_item.L1Text, _item.L2Text);
                _item.TokenisedText = vo.Document.ToString();

                return new ParserTokeniserDto()
                           {
                               Item = _item,
                               VideoPlaybackData = vo.PlaybackData
                           };
            }
            
            if(_item.ItemType == ItemType.Text)
            {
                item.TokenisedText = CreateTextXml(item.L1Text, item.L2Text).ToString();

                return new ParserTokeniserDto()
                {
                    Item = _item
                };
            }

            throw new NotSupportedException();
        }

        protected XDocument CreateTextXml(string text, string parallelText)
        {
            string[] paragraphs = text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] parallelParagraphs = parallelText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            XDocument document = new XDocument();
            var rootNode = new XElement("root");
            rootNode.SetAttributeValue("title", _item.Title);

            for(int i = 0; i < paragraphs.Length; i++)
            {
                var paragraph = paragraphs[i];
                var parallelParagraph = i < parallelParagraphs.Length ? parallelParagraphs[i] : "";

                var thisParagraph = CreateParagraph(paragraph, false);
                var thisParallelParagraph = CreateParagraph(parallelParagraph, true);

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

        protected ItemTokeniserOutput CreateVideoXml(string l1t, string l2t)
        {
            XDocument document = new XDocument();
            var rootNode = new XElement("root");
            rootNode.SetAttributeValue("title", _item.Title);

            IEnumerable<Subtitle> l1 = ParseSubtitleFile(l1t);
            IEnumerable<Subtitle> l2 = ParseSubtitleFile(l2t);
            var playbackData = new List<ParserOutput.VideoPlaybackData>();

            foreach(var subtitle in l1)
            {
                playbackData.Add(new ParserOutput.VideoPlaybackData()
                {
                    ElementId = "l1-" + subtitle.LineNumber,
                    FromSeconds = subtitle.FromSeconds,
                    ToSeconds = subtitle.ToSeconds,
                    L1 = true
                });

                var element = CreateParagraph(subtitle.Text, false);
                element.SetAttributeValue("language", "l1");
                element.SetAttributeValue("line", subtitle.LineNumber);
                rootNode.Add(element);
            }

            foreach(var subtitle in l2)
            {
                playbackData.Add(new ParserOutput.VideoPlaybackData()
                {
                    ElementId = "l2-" + subtitle.LineNumber,
                    FromSeconds = subtitle.FromSeconds,
                    ToSeconds = subtitle.ToSeconds,
                    L1 = false
                });

                var element = CreateParagraph(subtitle.Text, true);
                element.SetAttributeValue("language", "l2");
                element.SetAttributeValue("line", subtitle.LineNumber);
                rootNode.Add(element);
            }

            document.Add(rootNode);

            return new ItemTokeniserOutput()
                       {
                           Document = document,
                           PlaybackData = playbackData
                       };
        }

        protected XElement CreateParagraph(string paragraph, bool isParallel)
        {
            var thisParagraph = new XElement("p");
            string[] sentences = _sentenceSplitter.Split(paragraph);

            for(int i = 0; i < sentences.Length; i += 2)
            {
                string sentence = sentences[i];
                if(sentences.Length > i + 1) sentence += sentences[i + 1];
                if(string.IsNullOrEmpty(sentence.TrimEnd())) continue;

                var thisSentence = new XElement("s");

                string[] tokens = _tokenSplitter.Split(sentence);

                foreach(var token in tokens)
                {
                    if(string.IsNullOrEmpty(token) || token == "\r" || token == "\n") continue;
                    XElement t;

                    if(token == "\t")
                    {
                        t = new XElement("t");
                        t.SetAttributeValue("type", "tab");
                    }
                    else if(token == " ")
                    {
                        t = new XElement("t");
                        t.SetAttributeValue("type", "space");
                        t.SetAttributeValue("inSpan", isParallel ? "false" : "true");
                    }
                    else if(_language.Punctuation.Contains(token))
                    {
                        t = new XElement("t");
                        t.SetAttributeValue("type", "punctuation");
                        t.SetAttributeValue("inSpan", isParallel ? "false" : "true");
                        t.SetAttributeValue("value", token);
                    }
                    else
                    {
                        t = new XElement("t");

                        if(isParallel)
                        {
                            t.SetAttributeValue("type", "parallel");
                        }
                        else
                        {
                            t.SetAttributeValue("type", "word");
                            string lower = token.ToLowerInvariant();
                            t.SetAttributeValue("lower", lower);
                            t.SetAttributeValue("state", _notseenState);
                        }

                        t.SetAttributeValue("value", token);
                    }

                    thisSentence.Add(t);
                }

                thisParagraph.Add(thisSentence);
            }

            return thisParagraph;
        }

        protected IEnumerable<Subtitle> ParseSubtitleFile(string file)
        {
            IList<Subtitle> subtitles = new List<Subtitle>();
            Subtitle subtitle = null;
            foreach(var line in file.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray())
            {
                try
                {
                    if(string.IsNullOrEmpty(line)) continue;

                    int test;

                    if(int.TryParse(line, out test))
                    {
                        if(subtitle != null)
                        {
                            subtitles.Add(subtitle);
                        }
                        subtitle = new Subtitle();
                        subtitle.LineNumber = test;
                    }
                    else if(line.Contains(" --> "))
                    {
                        string[] times = line.Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                        DateTime start = DateTime.ParseExact(times.First(), "hh:mm:ss,fff", CultureInfo.InvariantCulture);
                        DateTime end = DateTime.ParseExact(times.Last(), "hh:mm:ss,fff", CultureInfo.InvariantCulture);
                        subtitle.FromSeconds = start.Hour * 60 * 60 + start.Minute * 60 + start.Second + (decimal)start.Millisecond / 1000;
                        subtitle.ToSeconds = end.Hour * 60 * 60 + end.Minute * 60 + end.Second + (decimal)end.Millisecond / 1000;
                    }
                    else
                    {
                        if(string.IsNullOrEmpty(subtitle.Text)) subtitle.Text = line;
                        else subtitle.Text += "\n" + line;
                    }
                }
                catch(Exception e)
                {
                    string message = "Could not parse: " + line + Environment.NewLine + e.Message;
                    throw new SubtitleParsingException(message);
                }
            }

            return subtitles;
        }
    }
}
