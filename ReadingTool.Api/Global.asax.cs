using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using ReadingTool.Api.Handler;
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
        }
    }
}