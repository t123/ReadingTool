using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Extensions;
using ReadingTool.Entities;
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Languages;
using ReadingTool.Site.Models.Terms;
using ReadingTool.Site.Models.Texts;

namespace ReadingTool.Site
{
    public class Mappings
    {
        public static void Register()
        {
            Mapper.CreateMap<UserDictionary, DictionaryModel>();
            Mapper.CreateMap<UserDictionary, DictionaryViewModel>();
            Mapper.CreateMap<Language, LanguageViewModel>();
            Mapper.CreateMap<Text, TextViewModel>()
                .ForMember(x => x.Title, y => y.MapFrom(z => (z.CollectionNo.HasValue ? string.Format("{0:00}. ", z.CollectionNo.Value) : "") + z.Title))
                .ForMember(x => x.Language1, y => y.MapFrom(z => z.Language1.Name))
                .ForMember(x => x.IsParallel, y => y.MapFrom(z => z.Language2 != null))
                .ForMember(x => x.Created, y => y.MapFrom(z => (DateTime.Now - z.Created).ToSince("ago")))
                .ForMember(x => x.Modified, y => y.MapFrom(z => (DateTime.Now - z.Modified).ToSince("ago")))
                .ForMember(x => x.LastRead, y => y.MapFrom(z => (DateTime.Now - z.LastRead).ToSince("ago", "")))
                ;

            Mapper.CreateMap<User, AccountModel.UserModel>();

            Mapper.CreateMap<Term, TermViewModel>()
                .ForMember(x => x.State, y => y.MapFrom(z => z.State.ToDescription()))
                .ForMember(x => x.Created, y => y.MapFrom(z => (DateTime.Now - z.Created).ToSince("ago")))
                .ForMember(x => x.Modified, y => y.MapFrom(z => (DateTime.Now - z.Modified).ToSince("ago")))
                .ForMember(x => x.Language, y => y.MapFrom(z => z.Language.Name))
                .ForMember(x => x.NextReviewDate, y => y.MapFrom(z => (z.NextReview - DateTime.Now).ToSince("", "no review date")))
                ;

            Mapper.CreateMap<Term, TermModel>()
                .ForMember(x => x.Tags, y => y.MapFrom(z => Tags.ToString(z.Tags.Select(w => w.TagTerm))))
                ;
        }
    }
}