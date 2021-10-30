using AECMIS.DAL.Domain;
using AECMIS.DAL.Nhibernate.Mappings;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class UserMap : BaseMap<User>
    {
        public UserMap()
        {
            Table("[user].[User]");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.UserName).Column("UserName");
            Map(x => x.Password).Column("Password");
            //HasManyToMany(x => x.Roles).Table("[user].UserRoles").
            //    ParentKeyColumn("UserId").ChildKeyColumns.Add(new[]
            //                                                      {"RoleId", "ModifiedDate", "ModifiedBy", "CreatedBy"})
            //    .Cascade.All().LazyLoad();
            HasMany(x => x.Roles).KeyColumn("UserId").Cascade.All().LazyLoad();

        }
    }
}
