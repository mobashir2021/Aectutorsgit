using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class StudentSubjectMap:BaseMap<StudentSubject>
    {
        public StudentSubjectMap()
        {
            Table("student.StudentSubjects");
            CompositeId(x => x.StudentSubjectId).KeyProperty(x => x.SessionId).KeyProperty(x => x.StudentId).KeyProperty(x => x.SubjectId);
            References(x => x.Session).Column("SessionId").ReadOnly();
            References(x => x.Student).Column("StudentId").ReadOnly();
            References(x => x.Subject).Column("SubjectId").ReadOnly();
           Version(x => x.Version)
           .CustomType("Timestamp");
        }
    }
}
