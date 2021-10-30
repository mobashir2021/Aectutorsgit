using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class TeacherMapping : BaseMap<Teacher>
    {
        public TeacherMapping()
        {
            Table("teacher.Teacher");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.FirstName).Column("FirstName");
            Map(x => x.LastName).Column("LastName");
            Map(x => x.MiddleName).Column("MiddleName");
            Map(x => x.Age).Column("Age");
        }
    }
}
