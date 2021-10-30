using System.Web.Mvc;
using System.Web.Routing;

namespace AECMIS.MVC.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Registers", "SessionRegister/{action}", new { controller = "SessionRegister", action = "Index" }

       );

            routes.MapRoute("GetRegister", "SessionRegister/{action}/{id}/{date}", new { controller = "SessionRegister", action = "Get",id=UrlParameter.Optional, date=UrlParameter.Optional }

        );

            routes.MapRoute("Students", "Student/{action}", new { controller = "Student", action = "Index" }

        );

            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Student", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}