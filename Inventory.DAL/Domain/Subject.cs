using System.Collections.Generic;
using System.Xml.Serialization;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class Subject : Entity
    {        
        public virtual string Name { get; set; }
        public virtual Curriculum Level { get; set; }
        public virtual IList<Session> SubjectTaughtInSessions { get; set; }
    }
}
