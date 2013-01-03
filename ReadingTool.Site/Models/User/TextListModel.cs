using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Models.User
{
    public class TextListModel
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public string LanguageColour { get; set; }
        public string Title { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool HasAudio { get; set; }
        public bool IsParallel { get; set; }
        public string Tags { get; set; }
        public int? TimesRead { get; set; }
        public long? WordsRead { get; set; }
        public int? TimesListened { get; set; }
        public string ListenLength { get; set; }
    }
}