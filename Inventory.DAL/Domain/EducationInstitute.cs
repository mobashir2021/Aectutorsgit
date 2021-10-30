using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.Contracts;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class EducationInstitute:Entity
    {
        public EducationInstitute()
        {
            Qualifications= new List<Qualification>();
        }

        public virtual string Name { get; set; }
        public virtual InstituteType Type { get; set; }
        public virtual IAddress Address { get; set; }
        public virtual string Teacher { get; set; }
        public virtual string StudentNo { get; set; }
        public virtual DateTime? From { get; set; }
        public virtual DateTime? To { get; set; }
        public virtual IList<Qualification> Qualifications { get; set; }
        public virtual Student Student { get; set; }
    }
}
