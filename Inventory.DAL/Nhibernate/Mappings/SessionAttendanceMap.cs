using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class SessionAttendanceMap:BaseMap<SessionAttendance> 
    {
        public SessionAttendanceMap()
        {
            Table("timetable.SessionAttendance");
            
            //Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            References(x => x.Student).Column("StudentId").Not.Nullable();
            References(x => x.Invoice).Column("InvoiceId");
            Map(x => x.Status).Column("Status");
            //Changes added -- Removed column
            //Map(x => x.RemainingCredits).Column("RemainingCredits");
            HasMany(x => x.SubjectsAttended).KeyColumn("SessionAttendanceId").LazyLoad().Cascade.AllDeleteOrphan().Inverse();
            HasMany(x => x.Payments).KeyColumn("AttendanceId").LazyLoad().Cascade.AllDeleteOrphan().Inverse();
            References(x => x.SessionRegister).Column("SessionRegisterId").Not.Nullable();
        } 

    }
}
