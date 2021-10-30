using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class SessionMapping:BaseMap<Session>
    {
        public SessionMapping()
        {
            Table("timetable.Session");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.To).Column("ToTime").CustomType("TimeAsTimeSpan").Not.Nullable();
            Map(x => x.From).Column("FromTime").CustomType("TimeAsTimeSpan").Not.Nullable();
            Map(x => x.Day).Column("Day").Not.Nullable();
            References(x => x.Location).Column("TutionCentre");
            HasManyToMany(x => x.SubjectsTaughtAtSession).Table("[timetable].SubjectsTaughtAtSession").
                ParentKeyColumn("SessionId").ChildKeyColumn("SubjectId").Cascade.All().LazyLoad();

        }

    }
}
