using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain
{
    public class Entity
    {
        public virtual int Id{get;set;}
        public virtual DateTime ModifiedDate { get; set; }
        public virtual int ModifiedBy { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedDate { get; set; }
    }
}
