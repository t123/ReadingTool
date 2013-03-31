#region License
// Global.asax.cs is part of ReadingTool.Api
// 
// ReadingTool.Api is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Api is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Api. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using ReadingTool.Api.Handler;
using ReadingTool.Api.Models;
using ReadingTool.Api.Models.Languages;
using ReadingTool.Api.Models.Terms;
using ReadingTool.Entities;

namespace ReadingTool.Api
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //GlobalConfiguration.Configuration.MessageHandlers.Add(new HttpsHandler());
            GlobalConfiguration.Configuration.MessageHandlers.Add(new BasicAuthenticationMessageHandler());
            //GlobalConfiguration.Configuration.MessageHandlers.Add(new CorsHandler());
            //GlobalConfiguration.Configuration.MessageHandlers.Add(new TokenMessageHandler());

            Mapper.CreateMap<Language, LanguageResponseModel>()
                .ForMember(x => x.Direction, y => y.MapFrom(z => z.Settings.Direction.ToString()))
                .ForMember(x => x.RegexSplitSentences, y => y.MapFrom(z => z.Settings.RegexSplitSentences))
                .ForMember(x => x.RegexWordCharacters, y => y.MapFrom(z => z.Settings.RegexWordCharacters))
                .ForMember(x => x.ShowSpaces, y => y.MapFrom(z => z.Settings.ShowSpaces))
                .ForMember(x => x.Modal, y => y.MapFrom(z => z.Settings.Modal))
                ;

            Mapper.CreateMap<Term, TermResponseModel>()
                .ForMember(x => x.Tags, y => y.MapFrom(z => z.Tags.Select(w => w.TagTerm)))
                .ForMember(x => x.State, y => y.MapFrom(z => z.State.ToString()))
                .ForMember(x => x.LanguageId, y => y.MapFrom(z => z.Language.LanguageId))
                .ForMember(x => x.TextId, y => y.MapFrom(z => z.Text.TextId))
                ;

            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}