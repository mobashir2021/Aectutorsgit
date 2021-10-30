using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AECMIS.MVC.App_Start;
using AECMIS.MVC.AutoMapper;
using AECMIS.MVC.Helpers;

//using RouteDebug;

namespace AECMIS.MVC
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            CultureInfo culture = CultureInfo.GetCultureInfo("en-GB");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            AreaRegistration.RegisterAllAreas();            
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            AutoMapperBootStrap.InitializeMap();
           // HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.
             //Initialize();
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            log4net.Config.XmlConfigurator.Configure();

            //2013-07-20
            // RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
            // .RouteDebuggerHttpModule.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            var code = (error is HttpException) ? (error as HttpException).GetHttpCode() : 500;

            ErrorLogger.LogError(error);
        }

    }
}