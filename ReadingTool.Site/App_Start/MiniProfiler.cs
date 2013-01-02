using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using ReadingTool.Core;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;
using Microsoft.Web.Infrastructure;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
//using System.Data;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using StackExchange.Profiling.Data.EntityFramework;
//using StackExchange.Profiling.Data.Linq2Sql;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(ReadingTool.Site.App_Start.MiniProfilerPackage), "PreStart")]

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(ReadingTool.Site.App_Start.MiniProfilerPackage), "PostStart")]


namespace ReadingTool.Site.App_Start
{
    public static class MiniProfilerPackage
    {
        public static void PreStart()
        {
            MiniProfiler.Settings.SqlFormatter = new StackExchange.Profiling.SqlFormatters.SqlServerFormatter();
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));
            GlobalFilters.Filters.Add(new ProfilingActionFilter());

            // You can use this to check if a request is allowed to view results
            //MiniProfiler.Settings.Results_Authorize = (request) =>
            //{
            // you should implement this if you need to restrict visibility of profiling on a per request basis 
            //    return !DisableProfilingResults; 
            //};

            // the list of all sessions in the store is restricted by default, you must return true to alllow it
            //MiniProfiler.Settings.Results_List_Authorize = (request) =>
            //{
            // you may implement this if you need to restrict visibility of profiling lists on a per request basis 
            //return true; // all requests are kosher
            //};
        }

        public static void PostStart()
        {
            // Intercept ViewEngines to profile all partial views and regular views.
            // If you prefer to insert your profiling blocks manually you can comment this out
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach(var item in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("MiniProfiler");

        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Request;
                MiniProfiler.Start();
            };

            context.EndRequest += (sender, e) =>
            {
                MiniProfiler miniProfiler = MiniProfiler.Current;

                if(miniProfiler != null)
                {
                    Logger.InfoFormat("profiling result id:{0}\nresult:{1}\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>", miniProfiler.Id, miniProfiler.Render());
                }

                if(context.User == null || (context.User != null && !context.User.IsInRole(Constants.Roles.ADMIN)))
                {
                    StackExchange.Profiling.MiniProfiler.Stop(discardResults: true);
                }
                else
                {
                    MiniProfiler.Stop();
                }
            };
        }

        public void Dispose() { }
    }
}

