using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class SessionRegisterMap:BaseMap<SessionRegister>
    {
        public SessionRegisterMap()
        {
            Table("[TimeTable].SessionRegister");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Date).Column("Date").CustomType("date");
            Map(x => x.Status).Column("Status");
            References(x => x.Session).Column("SessionId").Not.Nullable();
            References(x => x.Centre).Column("TuitionCentreId");
            HasMany(x => x.SessionAttendances).KeyColumn("SessionRegisterId").Cascade.AllDeleteOrphan().Inverse();
        }

    }
}
