using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain
{
    public class TeacherAttendance:Entity
    {
        public virtual DateTime Day { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual TimeSpan TimeStarted { get; set; }
        public virtual TimeSpan TimeEnded { get; set; }
        public virtual int NumberOfLessonsWorked { get; set; }
        public virtual double NumberOfHoursWorked { get; set; }
        public virtual int ExtraTimeInMins { get; set; }
        public virtual string ExtraWork { get; set; }

        public virtual Teacher TeacherCoveredFor { get; set; }
        public virtual decimal Paid { get; set; }

    }
}
