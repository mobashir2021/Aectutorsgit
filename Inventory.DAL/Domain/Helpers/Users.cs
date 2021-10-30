using System.Web;

namespace AECMIS.DAL.Domain.Helpers
{
    public class Users
    {
        public const string UserSessionKey = "UserId";

        public static int GetCurrentUser()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null ||
                HttpContext.Current.Session[UserSessionKey] == null
                || string.IsNullOrEmpty(HttpContext.Current.Session[UserSessionKey].ToString()))
            {
                //if (HttpContext.Current == null)
                //    Logger.Instance.Info("Falling back on System user because HttpContext.Current is null");
                //else
                //{
                //    if (HttpContext.Current.Session == null)
                //        Logger.Instance.Info("Falling back on System user because HttpContext.Current.Session is null");
                //    else
                //    {
                //        if (HttpContext.Current.Session[UserSessionKey] == null)
                //            Logger.Instance.Info("Falling back on System user because HttpContext.Current.Session[UserSessionKey] is null");
                //        else
                //        {
                //            if (string.IsNullOrEmpty(HttpContext.Current.Session[UserSessionKey].ToString()))
                //                Logger.Instance.Info("Falling back on System user because HttpContext.Current.Session[UserSessionKey].ToString() is null or empty");
                //        }
                //    }
                //}

                //return (int)UserIds.SystemUser;
            }

            if (HttpContext.Current == null)
                return 1;

            return (int) HttpContext.Current.Session[UserSessionKey];


        }
    }
}
