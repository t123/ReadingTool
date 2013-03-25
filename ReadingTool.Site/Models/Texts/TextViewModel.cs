using System;

namespace ReadingTool.Site.Models.Texts
{
    public class TextViewModel
    {
        public long TextId { get; set; }
        public string Title { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public string Language1 { get; set; }
        public string Created { get; set; }
        public string Modified { get; set; }
        public string LastRead { get; set; }
        public bool IsParallel { get; set; }
    }
}