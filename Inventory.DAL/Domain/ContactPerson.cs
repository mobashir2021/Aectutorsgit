using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class ContactPerson : Entity
    {
        public virtual string ContactName { get; set; }
        public virtual RelationType Type { get; set; }
        public virtual IAddress ContactAddress { get; set; }
        public virtual IContact ContactPhone { get; set; }
        public virtual Student Student { get; set; }
        public virtual bool IsPrimaryContact { get; set; }
        public virtual TitleTypes Title { get; set; }
    }
}
