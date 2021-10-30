using System.Collections.Generic;

namespace AECMIS.DAL.Domain
{
    public class User:Entity
    {
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }

        public virtual IList<UserRole> Roles { get; set; }
    }

    public class Role : Entity
    {
        public virtual string RoleName { get; set; }
    }
}
