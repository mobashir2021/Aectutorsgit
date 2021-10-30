using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class TuitionCentreMapping:BaseMap<TuitionCentre>
    {
        public TuitionCentreMapping()
        {
            Table("timetable.TuitionCentre");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Name).Column("Name");
            Map(x => x.Address).Column("Address");
        }
    }
}
