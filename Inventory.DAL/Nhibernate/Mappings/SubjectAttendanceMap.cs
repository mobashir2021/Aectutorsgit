using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class SubjectAttendanceMap : BaseMap<SubjectAttendance>
    {
        public SubjectAttendanceMap()
        {
            Table("timetable.SubjectAttendance");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            References(x => x.Attendance).Column("SessionAttendanceId");
            References(x => x.Subject).Column("SubjectId").Not.Nullable();
            References(x => x.Teacher).Column("TeacherId").Nullable();            
            Map(x => x.Notes).Column("Notes").Nullable();

        }
    }
}