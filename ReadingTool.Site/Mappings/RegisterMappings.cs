using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Core;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Mappings
{
    public class RegisterMappings
    {
        public static void Register()
        {
            Mapper.CreateMap<LanguageSettings, LanguageSettingsViewModel>();
            Mapper.CreateMap<LanguageSettingsViewModel, LanguageSettings>();

            Mapper.CreateMap<DictionaryViewModel, LanguageDictionary>();
            Mapper.CreateMap<LanguageDictionary, DictionaryViewModel>();

            Mapper.CreateMap<KeyBindings, KeyBindingsModel>();
            Mapper.CreateMap<KeyBindingsModel, KeyBindings>();
            Mapper.CreateMap<KeyModel, Key>();
            Mapper.CreateMap<Key, KeyModel>();


            //Mapper.CreateMap<Language, Models.WebApi.LanguageModel>()
            //    .ForMember(x => x.SystemLangcodeCode, y => y.MapFrom(z => z.SystemLanguageId == null ? "" : DependencyResolver.Current.GetService<ISystemLanguageService>().Find(z.SystemLanguageId.Value).Code))
            //    .ForMember(x => x.SystemLangcodeName, y => y.MapFrom(z => z.SystemLanguageId == null ? "" : DependencyResolver.Current.GetService<ISystemLanguageService>().Find(z.SystemLanguageId.Value).Name))
            //    ;

            //Mapper.CreateMap<LanguageSettings, Models.WebApi.LanguageSettingsModel>();
            //Mapper.CreateMap<LanguageDictionary, Models.WebApi.LanguageDictionaryModel>();

            //Mapper.CreateMap<Text, Models.WebApi.TextModel>()
            //    .ForMember(x => x.L1Name, y => y.MapFrom(z => DependencyResolver.Current.GetService<ILanguageService>().Find(z.L1Id).Name))
            //    .ForMember(x => x.L2Name, y => y.MapFrom(z => z.L2Id == null ? "" : DependencyResolver.Current.GetService<ILanguageService>().Find(z.L2Id.Value).Name))
            //    .ForMember(x => x.Tags, y => y.MapFrom(z => TagHelper.Split(z.Tags)))
            //    ;

            //Mapper.CreateMap<Term, Models.WebApi.TermModel>()
            //    .ForMember(x => x.LanguageName, y => y.MapFrom(z => DependencyResolver.Current.GetService<ILanguageService>().Find(z.LanguageId).Name))
            //    ;

            //Mapper.CreateMap<IndividualTerm, Models.WebApi.IndividualTermModel>();
        }
    }
}