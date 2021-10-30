using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class TeacherAttendanceMap:BaseMap<TeacherAttendance>
    {
        public TeacherAttendanceMap()
        {
            Table("[teacher].[TeacherAttendance]");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Day);
            Map(x => x.TimeStarted);
            Map(x => x.TimeEnded);
            Map(x => x.NumberOfLessonsWorked);
            Map(x => x.NumberOfHoursWorked);
            Map(x => x.Paid);
            Map(x => x.ExtraTimeInMins);
            Map(x => x.ExtraWork);
            References(x => x.TeacherCoveredFor).Column("TeacherCoveredFor");
            References(x => x.Teacher).Column("TeacherId");
        }
    }
}
