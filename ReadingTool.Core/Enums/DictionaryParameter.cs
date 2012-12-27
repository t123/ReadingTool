using System.ComponentModel;

namespace ReadingTool.Core.Enums
{
    public enum DictionaryParameter : short
    {
        [Description("Dictionary Lookup")]
        Word = 1,

        [Description("Sentence Translation")]
        Sentence = 2
    }
}