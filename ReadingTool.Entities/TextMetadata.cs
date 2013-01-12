using System;

namespace ReadingTool.Entities
{
    public class TextMetadata
    {
        public DateTime? LastSeen { get; set; }
        public int? TimesRead { get; set; }
        public int? TimesListened { get; set; }
        public long? AudioLength { get; set; }
        public long? ListenLength { get; set; }
        public long? WordsRead { get; set; }
    }
}