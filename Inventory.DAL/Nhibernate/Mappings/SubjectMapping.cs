using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class SubjectMapping:BaseMap<Subject>
    {
        public SubjectMapping()
        {
            Table("timetable.Subject");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Level).Column("Level");
            Map(x => x.Name).Column("Name");
            HasManyToMany(x => x.SubjectTaughtInSessions).Table("[timetable].SubjectsTaughtAtSession").
                ChildKeyColumn("SessionId").ParentKeyColumn("SubjectId").Inverse().Cascade.All().LazyLoad();
        }
    }
}
