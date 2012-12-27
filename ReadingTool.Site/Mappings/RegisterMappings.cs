using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
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
        }
    }
}