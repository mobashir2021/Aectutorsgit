using AECMIS.MVC.ErrorHandler;
using System.Web;
using System.Web.Mvc;

namespace AECMIS.MVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AiHandleErrorAttribute()); 
        }
    }
}