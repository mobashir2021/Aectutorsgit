using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class QualificationMapping:BaseMap<Qualification>
    {
        public QualificationMapping()
        {
            Table("student.Qualification");
            Id(x=> x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Result).Column("Result");
            Map(x => x.Year).Column("Year");
            Map(x => x.Subject).Column("Subject");
            References(x => x.AchievedAtInstitute).Column("InstituteId");

        }
    }
}
