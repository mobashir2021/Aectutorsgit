using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class Qualification : Entity
    {
        public virtual string Subject { get; set; }
        public virtual string Result { get; set; }
        public virtual EducationInstitute AchievedAtInstitute { get; set; }
        public virtual int? Year { get; set; }
    }

}
