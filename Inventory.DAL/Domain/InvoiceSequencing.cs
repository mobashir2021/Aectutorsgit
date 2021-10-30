using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain
{
    public class InvoiceSequencing:Entity
    {   
        public virtual int FiscalYear { get; set; }
        public virtual DateTime FromDate { get; set; }
        public virtual DateTime ToDate { get; set; }
        public virtual int SequenceStartNum { get; set; }
        public virtual int SequenceEndNum { get; set; }

    }

    public class CreditNoteSequencing : Entity
    {
        public virtual int FiscalYear { get; set; }
        public virtual DateTime FromDate { get; set; }
        public virtual DateTime ToDate { get; set; }
        public virtual int SequenceStartNum { get; set; }
        public virtual int SequenceEndNum { get; set; }

    }
}
