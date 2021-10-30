namespace AECMIS.DAL.Domain
{
   public class UserRole:Entity
    {
       public virtual Role Role { get; set; }
       public virtual User User { get; set; }

    }
}
