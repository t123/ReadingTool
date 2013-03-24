using System;
using System.IO;
using System.Text;
using FluentNHibernate;
using FluentNHibernate.Mapping;
using Ionic.Zlib;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Text
    {
        public virtual Guid TextId { get; set; }
        public virtual string Title { get; set; }
        public virtual string CollectionName { get; set; }
        public virtual int? CollectionNo { get; set; }
        public virtual Language Language1 { get; set; }
        public virtual Language Language2 { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual DateTime? LastRead { get; set; }
        public virtual User User { get; set; }

        private byte[] _l1Text;
        private byte[] _l2Text;

        public virtual string L1Text
        {
            get
            {
                using(GZipStream stream = new GZipStream(new MemoryStream(_l1Text), CompressionMode.Decompress))
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

                    _l1Text = ms.ToArray();
                }
            }
        }

        public virtual string L2Text
        {
            get
            {
                using(GZipStream stream = new GZipStream(new MemoryStream(_l2Text), CompressionMode.Decompress))
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

                    _l2Text = ms.ToArray();
                }
            }
        }


        public virtual string AudioUrl { get; set; }
    }

    public class TextMap : ClassMap<Text>
    {
        public TextMap()
        {
            Id(x => x.TextId).GeneratedBy.GuidComb();
            Map(x => x.Title).Length(250).Not.Nullable();
            Map(x => x.CollectionName).Length(50);
            Map(x => x.CollectionNo);
            References(x => x.Language1);
            References(x => x.Language2);
            References(x => x.User).Not.Nullable();
            Map(x => x.Created).Not.Nullable();
            Map(x => x.Modified).Not.Nullable();
            Map(x => x.LastRead);
            Map(x => x.AudioUrl).Length(250);
            Map(Reveal.Member<Text>("_l1Text")).Column("L1Text").CustomSqlType("VARBINARY (MAX)").Length(2147483647).Not.Nullable();
            Map(Reveal.Member<Text>("_l2Text")).Column("L2Text").CustomSqlType("VARBINARY (MAX)").Length(2147483647).Not.Nullable();
        }
    }
}