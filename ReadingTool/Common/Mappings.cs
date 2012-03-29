#region License
// Mappings.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Areas.Admin.Models;
using ReadingTool.Common.Enums;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Entities.LWT;
using ReadingTool.Models.Create.Group;
using ReadingTool.Models.Create.LWT;
using ReadingTool.Models.Create.Language;
using ReadingTool.Models.Create.Text;
using ReadingTool.Models.Create.User;
using ReadingTool.Models.Create.Video;
using ReadingTool.Models.Create.Word;
using ReadingTool.Models.View.Group;
using ReadingTool.Models.View.Language;
using ReadingTool.Models.View.Message;
using ReadingTool.Models.View.TextParser;
using ReadingTool.Models.View.User;
using ReadingTool.Models.View.Word;
using ReadingTool.Services;

namespace ReadingTool.Common
{
    public static class Mappings
    {
        public static void ViewMappings()
        {
            Mapper.CreateMap<User, UserSimpleModel>().ForMember(x => x.Name, y => y.MapFrom(z => z.Fullname));
            Mapper.CreateMap<Message, MessageViewModel>()
                .ForMember(x => x.Body, y => y.MapFrom(z => MarkdownHelper.Default().Transform(z.Body)));

            Mapper.CreateMap<Group, GroupViewModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));
            Mapper.CreateMap<Word, WordViewModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));
            Mapper.CreateMap<GroupMember, GroupMemberViewModel>();
            Mapper.CreateMap<Language, LanguageSimpleViewModel>();
            Mapper.CreateMap<TextParser, TextParserViewModel>();

            Mapper.CreateMap<User, PublicProfileViewModel>()
                .ForMember(x => x.Availability, y => y.MapFrom(z => z.PublicProfile.Availability))
                .ForMember(x => x.ShowStats, y => y.MapFrom(z => z.PublicProfile.ShowStats))
                .ForMember(x => x.ShowNativeLanguage, y => y.MapFrom(z => z.PublicProfile.ShowNativeLanguage))
                .ForMember(x => x.Location, y => y.MapFrom(z => z.PublicProfile.Location))
                .ForMember(x => x.WebsiteUrl, y => y.MapFrom(z => z.PublicProfile.WebsiteUrl))
                .ForMember(x => x.TwitterUrl, y => y.MapFrom(z => z.PublicProfile.TwitterUrl))
                .ForMember(x => x.AboutMe, y => y.MapFrom(z => z.PublicProfile.AboutMe))
                .ForMember(x => x.Joined, y => y.MapFrom(z => DateTime.Now - z.Created))
                .ForMember(x => x.NativeLanguage, y => y.MapFrom(z => z.NativeLanguageId == null ? "" : (DependencyResolver.Current.GetService<ISystemLanguageService>()).FindOne(z.NativeLanguageId.Value).Name))
                ;

            Mapper.CreateMap<GroupShareNotice, GroupShareNoticeViewModel>();
            Mapper.CreateMap<Word, WordShareModel>();
        }

        public static void CreateMappings()
        {
            Mapper.CreateMap<LwtModel, Lwt>();
            Mapper.CreateMap<LwtJsonModel, Lwt>();

            Mapper.CreateMap<User, ProfileModel>()
                .ForMember(x => x.NativeLanguage, y => y.MapFrom(
                    z => z.NativeLanguageId.HasValue
                        ? DependencyResolver.Current.GetService<ISystemLanguageService>().FindOne(z.NativeLanguageId.Value).Name
                        : ""
                    )
                );

            Mapper.CreateMap<User, UserCookieModel>();
            Mapper.CreateMap<MediaControl, MediaControlModel>();
            Mapper.CreateMap<MediaControlModel, MediaControl>();
            Mapper.CreateMap<User, PublicProfileModel>()
                .ForMember(x => x.Availability, y => y.MapFrom(z => z.PublicProfile.Availability))
                .ForMember(x => x.ShowStats, y => y.MapFrom(z => z.PublicProfile.ShowStats))
                .ForMember(x => x.ShowNativeLanguage, y => y.MapFrom(z => z.PublicProfile.ShowNativeLanguage))
                .ForMember(x => x.Location, y => y.MapFrom(z => z.PublicProfile.Location))
                .ForMember(x => x.WebsiteUrl, y => y.MapFrom(z => z.PublicProfile.WebsiteUrl))
                .ForMember(x => x.TwitterUrl, y => y.MapFrom(z => z.PublicProfile.TwitterUrl))
                .ForMember(x => x.AboutMe, y => y.MapFrom(z => z.PublicProfile.AboutMe))
                ;

            Mapper.CreateMap<PublicProfileModel, PublicProfile>();

            Mapper.CreateMap<Style, StyleModel>();
            Mapper.CreateMap<StyleModel, Style>();

            Mapper.CreateMap<GroupModel, Group>()
               .ForMember(x => x.Tags, y => y.MapFrom(z => (TagHelper.Split(z.Tags))));
            Mapper.CreateMap<Group, GroupModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));
            Mapper.CreateMap<GroupMember, GroupMemberModel>();

            //Warning, make sure the name is valid before calling!
            Mapper.CreateMap<LanguageModel, Language>()
                .ForMember(x => x.SystemLanguageId, y => y.MapFrom(z => (DependencyResolver.Current.GetService<ISystemLanguageService>()).FindByName(z.SystemLanguageName).SystemLanguageId))
                .ForMember(x => x.Punctuation, y => y.MapFrom(z => z.Punctuation.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)))
                ;

            Mapper.CreateMap<Language, LanguageModel>()
                .ForMember(x => x.SystemLanguageName, y => y.MapFrom(z => (DependencyResolver.Current.GetService<ISystemLanguageService>()).FindOne(z.SystemLanguageId).Name))
                .ForMember(x => x.Punctuation, y => y.MapFrom(z => string.Join(" ", z.Punctuation)))
            ;

            Mapper.CreateMap<UserDictionaryModel, UserDictionary>();
            Mapper.CreateMap<UserDictionary, UserDictionaryModel>();

            Mapper.CreateMap<TextModel, Item>()
                .ForMember(x => x.Tags, y => y.MapFrom(z => (TagHelper.Split(z.Tags))))
                .ForMember(x => x.ItemType, y => y.UseValue(ItemType.Text))
                .ForMember(x => x.ParseWith, y => y.Ignore())
                ;

            Mapper.CreateMap<VideoModel, Item>()
                .ForMember(x => x.Tags, y => y.MapFrom(z => (TagHelper.Split(z.Tags))))
                .ForMember(x => x.ItemType, y => y.UseValue(ItemType.Video))
                ;

            Mapper.CreateMap<Item, TextModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));
            Mapper.CreateMap<Item, VideoModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));

            Mapper.CreateMap<WordModel, Word>()
                .ForMember(x => x.Tags, y => y.MapFrom(z => (TagHelper.Split(z.Tags))));
            Mapper.CreateMap<Word, WordModel>().ForMember(x => x.Tags, y => y.MapFrom(z => string.Join(TagHelper.TAG_SEPARATOR, z.Tags)));
        }

        private static void AdminMappings()
        {
            Mapper.CreateMap<EmailTemplate, EmailTemplateModel>();

            Mapper.CreateMap<TextParser, TextParserModel>();
            Mapper.CreateMap<TextParserModel, TextParser>();

            Mapper.CreateMap<SystemLanguage, SystemLanguageModel>();

            Mapper.CreateMap<SystemTaskModel, SystemTask>();
            Mapper.CreateMap<SystemTask, SystemTaskModel>();
        }

        public static void RegisterMappings()
        {
            ViewMappings();
            CreateMappings();
            AdminMappings();
        }
    }
}