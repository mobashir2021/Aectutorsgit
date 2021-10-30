using AECMIS.DAL.Domain.Contracts;

namespace AECMIS.DAL.Domain
{
    public class Teacher : Entity, IPerson
    {
        
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual int? Age { get; set; }
    }

}
