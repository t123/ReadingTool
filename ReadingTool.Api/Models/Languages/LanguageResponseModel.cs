using ReadingTool.Common;

namespace ReadingTool.Api.Models.Languages
{
    public class LanguageResponseModel
    {
        public virtual long LanguageId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Code { get; set; }
        public bool ShowSpaces { get; set; }
        public bool Modal { get; set; }
        public string RegexSplitSentences { get; set; }
        public string RegexWordCharacters { get; set; }
        public string Direction { get; set; }
    }
}