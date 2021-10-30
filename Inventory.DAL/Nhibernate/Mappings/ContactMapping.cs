using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class ContactMapping:BaseMap<Contact>
    {
        public ContactMapping()
        {
            Table("contact.Contact");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Email).Column("Email");
            Map(x => x.HomeNumber).Column("HomeNumber");
            Map(x => x.MobileNumber).Column("MobileNumber");
            Map(x => x.WorkNumber).Column("WorkNumber");
        }
    }
}
