using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class AddressMap:BaseMap<Address>
    {
        public AddressMap()
        {
            Table("contact.Address");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.AddressLine1).Column("AddressLine1");
            Map(x => x.AddressLine2).Column("AddressLine2");
            Map(x => x.AddressLine3).Column("AddressLine3");
            Map(x => x.AddressLine4).Column("AddressLine4");
            Map(x => x.AddressLine5).Column("AddressLine5");
            Map(x => x.City).Column("City");
            Map(x => x.County).Column("County");
            Map(x => x.PostCode).Column("PostCode");

        }
    }
}
