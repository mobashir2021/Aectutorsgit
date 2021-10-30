using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain
{
    public class Session : Entity
    {
        public Session()
        {
            SubjectsTaughtAtSession = new List<Subject>();
        }
        public virtual DayOfWeek Day { get; set; }
        public virtual TimeSpan From { get; set; }
        public virtual TimeSpan To { get; set; }
        public virtual TuitionCentre Location { get; set; }
        public virtual IList<Subject> SubjectsTaughtAtSession { get; set; }
        public virtual string Identifier { get; set; }
    }
}
