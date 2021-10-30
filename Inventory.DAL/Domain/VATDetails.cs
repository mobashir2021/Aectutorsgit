using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain
{
    public class VATDetails:Entity
    {
        public VATDetails()
        {

        }
        public virtual int FiscalYear { get; set; }
        public virtual DateTime FromDate { get; set; }
        public virtual DateTime ToDate { get; set; }
        public virtual decimal VATPercentage { get; set; }
    }
}
