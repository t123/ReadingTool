using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReadingTool.Entities
{
    public class Statistics
    {
        public const string CollectionName = @"Statistics";
        public static int[] Period = new int[] { 0, 30, 60, 90, 120, 150, 180, 100000 };

        [BsonId]
        public ObjectId Owner { get; set; }
        public DateTime Modified { get; set; }
        public IList<Details> Languages { get; set; }

        public class Details
        {
            public ObjectId LanguageId { get; set; }
            public long TotalKnownWords { get; set; }
            public long TotalUnknownWords { get; set; }
            public long[] KnownPeriod { get; set; }
            public long[] UnknownPeriod { get; set; }

            public Details()
            {
                KnownPeriod = new long[Period.Length-1];
                UnknownPeriod = new long[Period.Length-1];
            }
        }

        public Statistics()
        {
            Languages = new List<Details>();
        }
    }
}
