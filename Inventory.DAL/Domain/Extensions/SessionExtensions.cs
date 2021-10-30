using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain.Extensions
{
    public static class SessionExtensions
    {

        public static string GetCentreInitials(TuitionCentre tuitionCentre)
        {
            //return tuitionCentre.Id == 1 ? "EHC" : "WC";
            return tuitionCentre.Name;
        }

        public static string SessionInfo(this Session session)
        {

            return string.Format("Session {0} with code {1} between {2} - {3} on {4}", session.Identifier, GetCentreInitials(session.Location), 
               session.From.HoursAndMins(), session.To.HoursAndMins(),session.Day);
        }
    }

    public static class SessionRegisterExtensions
    {
        public static string RegisterInfo(this SessionRegister register)
        {
            
            return string.Format("Session {0} at {1} on {2} {3}@{4} - {5}", register.Session.Identifier,
                                 SessionExtensions.GetCentreInitials(register.Centre),
                                 register.Session.Day,
                                 register.Date.ToString("yyyy-MM-dd"), register.Session.From.HoursAndMins(),
                                 register.Session.To.HoursAndMins());
        }
    }
}
