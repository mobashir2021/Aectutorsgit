using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class EducationInstituteMapping:BaseMap<EducationInstitute>
    {
        public EducationInstituteMapping()
        {
            Table("[student].[EducationInstitute]");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Type).Column("InstituteType");
            Map(x => x.From).Column("[From]");
            Map(x => x.To).Column("[To]").Nullable();
            Map(x => x.StudentNo).Column("Ref");
            Map(x => x.Name).Column("Name");
            Map(x => x.Teacher).Column("Teacher");
            References<Address>(x => x.Address).Column("Address").Cascade.All();
            References(x => x.Student).Column("StudentId");
            HasMany(x => x.Qualifications).KeyColumn("InstituteId").Cascade.AllDeleteOrphan().Inverse();
            

        }
    }
}
