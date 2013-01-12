using System;
using MongoDB.Bson;

namespace ReadingTool.Entities
{
    public class TermStatResult
    {
        public ObjectId Id { get; set; }
        public int Count { get; set; }
        public string State { get; set; }
    }

    public class TextStatResult
    {
        public ObjectId Id { get; set; }
        public int Count { get; set; }
        public long Listened { get; set; }
        public long Read { get; set; }
    }
}