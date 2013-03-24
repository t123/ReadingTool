using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Entities
{
    public class TextImport
    {
        public class JsonDefaults
        {
            public string L1LanguageName { get; set; }
            public string L2LanguageName { get; set; }
            public string CollectionName { get; set; }
            public bool? AutoNumberCollection { get; set; }
            public int? StartCollectionWithNumber { get; set; }

            public JsonDefaults()
            {
                L1LanguageName = string.Empty;
                L2LanguageName = string.Empty;
                CollectionName = string.Empty;
                StartCollectionWithNumber = null;
                AutoNumberCollection = false;
            }
        }

        public class JsonTextItem
        {
            public string L1LanguageName { get; set; }
            public string L2LanguageName { get; set; }
            public string Title { get; set; }
            public string AudioUrl { get; set; }
            public string CollectionName { get; set; }
            public int? CollectionNo { get; set; }
            public string L1Text { get; set; }
            public string L2Text { get; set; }
        }

        public JsonDefaults Defaults { get; set; }
        public JsonTextItem[] Items { get; set; }
    }
}
