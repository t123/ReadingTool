#region License
// Item.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReadingTool.Common.Enums;

namespace ReadingTool.Entities
{
    public class Item
    {
        public const string DbCollectionName = @"Items";

        [BsonId]
        public ObjectId ItemId { get; set; }
        public ObjectId LanguageId { get; set; }
        public ObjectId SystemLanguageId { get; set; }

        private string _collectionName;

        public string CollectionName
        {
            get { return _collectionName; }
            set
            {
                _collectionName = value ?? "";
                CollectionNameLower = _collectionName.ToLowerInvariant();
            }
        }

        public string CollectionNameLower { get; private set; }

        public int? CollectionNo { get; set; }
        public ItemType ItemType { get; set; }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value ?? "";
                TitleLower = _title.ToLowerInvariant();
            }
        }

        public string TitleLower { get; private set; }
        public string Url { get; set; }
        public bool ShareUrl { get; set; }
        private string[] _tags;
        public string[] Tags { get { return _tags ?? new string[0]; } set { _tags = value; } }

        public byte[] L1TextCompressed { get; private set; }
        public byte[] L2TextCompressed { get; private set; }

        [BsonIgnore]
        public string L1Text
        {
            get
            {
                using(GZipStream stream = new GZipStream(new MemoryStream(L1TextCompressed), CompressionMode.Decompress))
                {
                    const int size = 262144;
                    byte[] buffer = new byte[size];
                    using(MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if(count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while(count > 0);
                        return Encoding.UTF8.GetString(memory.ToArray());
                    }
                }
            }
            set
            {
                Byte[] bytes = Encoding.UTF8.GetBytes(value);

                using(MemoryStream ms = new MemoryStream())
                {
                    using(GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                        gzip.Close();
                    }

                    L1TextCompressed = ms.ToArray();
                }
            }
        }

        [BsonIgnore]
        public string L2Text
        {
            get
            {
                using(GZipStream stream = new GZipStream(new MemoryStream(L2TextCompressed), CompressionMode.Decompress))
                {
                    const int size = 262144;
                    byte[] buffer = new byte[size];
                    using(MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if(count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while(count > 0);
                        return Encoding.UTF8.GetString(memory.ToArray());
                    }
                }
            }
            set
            {
                IsParallel = !string.IsNullOrWhiteSpace(value);
                Byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");

                using(MemoryStream ms = new MemoryStream())
                {
                    using(GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(bytes, 0, bytes.Length);
                        gzip.Close();
                    }

                    L2TextCompressed = ms.ToArray();
                }
            }
        }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ObjectId Owner { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsParallel { get; set; }
        public bool ParallelIsRtl { get; set; }

        [BsonIgnore]
        public string TokenisedText { get; set; }

        private ObjectId[] _groupId;
        public ObjectId[] GroupId { get { return _groupId ?? new ObjectId[0]; } set { _groupId = value; } }

        private string _languageName;

        public string LanguageName
        {
            get { return _languageName; }
            set
            {
                _languageName = value ?? "";
                LanguageNameLower = _languageName.ToLowerInvariant();
            }
        }

        public string LanguageNameLower { get; private set; }

        public string LanguageColour { get; set; }

        public ObjectId ParseWith { get; set; }

        public Item Clone()
        {
            return new Item()
                       {
                           ItemId = this.ItemId,
                           LanguageId = this.LanguageId,
                           CollectionName = this.CollectionName,
                           CollectionNo = this.CollectionNo,
                           Title = this.Title,
                           Url = this.Url,
                           Tags = this.Tags,
                           L1Text = this.L1Text,
                           L2Text = this.L2Text,
                           Created = this.Created,
                           Modified = this.Modified,
                           Owner = this.Owner,
                           LastSeen = this.LastSeen,
                           IsParallel = this.IsParallel,
                           ParallelIsRtl = this.ParallelIsRtl,
                           TokenisedText = this.TokenisedText,
                           GroupId = this.GroupId,
                           LanguageName = this.LanguageName,
                           LanguageColour = this.LanguageColour,
                           ShareUrl = this.ShareUrl
                       };
        }

        public bool ShouldSerializeParseWith()
        {
            return ParseWith != ObjectId.Empty;
        }

        public bool ShouldSerializeTags()
        {
            return Tags != null && Tags.Length > 0;
        }

        public bool ShouldSerializeGroupId()
        {
            return GroupId != null && GroupId.Length > 0;
        }

        public Item()
        {
        }
    }
}
