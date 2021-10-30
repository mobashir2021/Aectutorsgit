using AECMIS.DAL.Domain;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class UserRoleMap : BaseMap<UserRole>
    {
        public UserRoleMap()
        {
            Table("[user].UserRoles");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            References(x => x.User).Column("UserId");
            References(x => x.Role).Column("RoleId");

        }
    }
}