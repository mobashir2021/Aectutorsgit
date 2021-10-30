using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class ContactPersonMapping:BaseMap<ContactPerson>
    {
        public ContactPersonMapping()
        {
            Table("contact.ContactPerson");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.ContactName).Column("Name");
            Map(x => x.Type).Column("Relation");
            Map(x => x.IsPrimaryContact).Column("IsPrimaryContact").Not.Nullable();
            Map(x => x.Title).Column("Title");
            References<Address>(x => x.ContactAddress).Column("AddressId").Cascade.All();
            References<Contact>(x => x.ContactPhone).Column("ContactId").Cascade.All();
            References(x => x.Student).Column("StudentId");

        }
    }
}
