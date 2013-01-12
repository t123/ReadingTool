using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Site.Models.WebApi
{
    public class TextModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public Guid L1Id { get; set; }
        public string L1Name { get; set; }
        public Guid? L2Id { get; set; }
        public string L2Name { get; set; }
        public string Title { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public string AudioUrl { get; set; }
        public DateTime? LastSeen { get; set; }
        public string L1Text { get; set; }
        public string L2Text { get; set; }
        public bool IsParallel { get; set; }
        public int? TimesRead { get; set; }
        public int? TimesListened { get; set; }
        public long? AudioLength { get; set; }
        public long? ListenLength { get; set; }
        public long? WordsRead { get; set; }
        public string[] Tags { get; set; }
    }
}
