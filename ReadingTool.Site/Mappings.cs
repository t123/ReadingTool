#region License
// Mappings.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Linq;
using System.Web;
using AutoMapper;
using ReadingTool.Common;
using ReadingTool.Common.Extensions;
using ReadingTool.Entities;
using ReadingTool.Site.Models.Account;
using ReadingTool.Site.Models.Admin;
using ReadingTool.Site.Models.Groups;
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
            Mapper.CreateMap<Language, LanguageViewModel>()
                .ForMember(x => x.Modal, y => y.MapFrom(z => z.Settings.Modal))
                .ForMember(x => x.ShowSpaces, y => y.MapFrom(z => z.Settings.ShowSpaces))
                .ForMember(x => x.ModalBehaviour, y => y.MapFrom(z => z.Settings.ModalBehaviour))
                ;

            Mapper.CreateMap<Text, TextViewModel>()
                .ForMember(x => x.Title, y => y.MapFrom(z => (z.CollectionNo.HasValue ? string.Format("{0:00}. ", z.CollectionNo.Value) : "") + z.Title))
                .ForMember(x => x.Language1, y => y.MapFrom(z => z.Language1.Name))
                .ForMember(x => x.IsParallel, y => y.MapFrom(z => z.Language2 != null))
                .ForMember(x => x.Created, y => y.MapFrom(z => (DateTime.Now - z.Created).ToSince("ago")))
                .ForMember(x => x.Modified, y => y.MapFrom(z => (DateTime.Now - z.Modified).ToSince("ago")))
                .ForMember(x => x.LastRead, y => y.MapFrom(z => (DateTime.Now - z.LastRead).ToSince("ago", "")))
                ;

            Mapper.CreateMap<User, UserModel>();

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

            Mapper.CreateMap<SystemLanguage, SystemLanguageModel>();
            Mapper.CreateMap<SystemLanguage, SystemLanguageIndexModel>();

            Mapper.CreateMap<Group, GroupViewModel>();
            Mapper.CreateMap<Text, GroupTextViewModel>()
                .ForMember(x => x.Title, y => y.MapFrom(z => (z.CollectionNo.HasValue ? string.Format("{0:00}. ", z.CollectionNo.Value) : "") + z.Title))
                .ForMember(x => x.Language1, y => y.MapFrom(z => z.Language1.Code))
                .ForMember(x => x.IsParallel, y => y.MapFrom(z => z.Language2 != null))
                .ForMember(x => x.User, y => y.MapFrom(z => z.User.DisplayName))
                ;
        }
    }
}