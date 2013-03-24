using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;
using ReadingTool.Common;

namespace ReadingTool.Entities
{
    public class Term
    {
        public virtual Guid TermId { get; set; }
        public virtual TermState State { get; set; }
        public virtual string Phrase { get; set; }
        public virtual string BasePhrase { get; set; }
        public virtual string Sentence { get; set; }
        public virtual string Definition { get; set; }
        public virtual short Box { get; set; }
        public virtual DateTime? NextReview { get; set; }
        public virtual Text Text { get; set; }
        public virtual Language Language { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual IList<Tag> Tags { get; set; }
        public virtual User User { get; set; }
        public virtual short Length { get; set; }

        public Term()
        {
            Length = 1;
            Box = 1;
            Tags = new List<Tag>();
        }

        public virtual string FullDefinition
        {
            get
            {
                string definition = string.Empty;

                if(!string.IsNullOrEmpty(BasePhrase)) definition += BasePhrase + "\n";
                if(!string.IsNullOrEmpty(Definition)) definition += Definition;
                if(Tags.Count > 0)
                {
                    definition += "\n" + string.Join(" ", Tags.Select(x => x.TagTerm));
                }

                return definition;
            }
        }

        public static string TermStateToClass(TermState state)
        {
            switch(state)
            {
                case TermState.Known:
                    return "_k";
                case TermState.Ignore:
                    return "_i";
                case TermState.NotKnown:
                    return "_u";
                case TermState.NotSeen:
                    return "_n";

                default:
                    throw new Exception("Unknown state: " + state);
            }
        }

        public static Tuple<TermState, DateTime?> NextReviewDate(Term term)
        {
            if(term.State == TermState.NotKnown)
            {
                Random r = new Random();
                int random;
                switch(term.Box)
                {
                    case 0: //Just in case
                    case 1:
                        random = r.Next(20) - 10;
                        return new Tuple<TermState, DateTime?>(TermState.NotKnown, DateTime.Now.AddMinutes(30 + random));
                    case 9:
                        return new Tuple<TermState, DateTime?>(TermState.Known, DateTime.Now.AddYears(10));
                    default:
                        random = r.Next(360) - 180;
                        var minutes = (Math.Pow(2, term.Box) * 24 * 60) + random;
                        return new Tuple<TermState, DateTime?>(TermState.NotKnown, DateTime.Now.AddMinutes(minutes));
                }
            }
            else
            {
                return new Tuple<TermState, DateTime?>(term.State, null);
            }
        }
    }

    public class TermMap : ClassMap<Term>
    {
        public TermMap()
        {
            Id(x => x.TermId).GeneratedBy.GuidComb();
            Map(x => x.Length).Not.Nullable();
            Map(x => x.State).CustomType<TermState>();
            Map(x => x.Phrase).Length(50).Not.Nullable();
            Map(x => x.BasePhrase).Length(50);
            Map(x => x.Sentence).Length(500);
            Map(x => x.Definition).Length(500);
            References(x => x.Text);
            References(x => x.Language).Not.Nullable();
            Map(x => x.Created);
            Map(x => x.Modified);
            References(x => x.User).Not.Nullable();
            Map(x => x.Box);
            Map(x => x.NextReview);

            HasManyToMany<Tag>(x => x.Tags)
                .Table("TermTag")
                .ParentKeyColumn("TermId")
                .ChildKeyColumn("TagId")
                .Cascade
                .All()
                .BatchSize(1000)
                ;
        }
    }
}