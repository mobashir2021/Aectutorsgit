using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UsersHelper = AECMIS.DAL.Domain.Helpers ;

namespace AECMIS.MVC.Filters
{
    public class AuthorizedWithSessionAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
           
            if (httpContext.Request.IsAuthenticated &&
                httpContext.Session[UsersHelper.Users.UserSessionKey] != null)
                return true;

            // sign them out so they can log back in with the Password
            if (httpContext.Request.IsAuthenticated)
                FormsAuthentication.SignOut();

            return false;
        }
    }
}