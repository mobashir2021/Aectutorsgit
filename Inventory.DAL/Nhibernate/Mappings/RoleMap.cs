using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class RoleMap : BaseMap<Role>
    {
        public RoleMap()
        {
            Table("[user].Roles");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.RoleName).Column("RoleName");
        }

    }
}