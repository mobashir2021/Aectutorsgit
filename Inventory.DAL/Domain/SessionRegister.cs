using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class SessionRegister:Entity
    {
        public SessionRegister()
        {
            SessionAttendances = new List<SessionAttendance>();
        }

        public virtual Session Session { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual IList<SessionAttendance> SessionAttendances { get; set; }
        public virtual SessionRegisterStatus Status { get; set; }
        public virtual TuitionCentre Centre { get; set; }

    }
}
