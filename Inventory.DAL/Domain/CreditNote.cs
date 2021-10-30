using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain
{
    public class CreditNote:Entity
    {
        public CreditNote()
        {

        }

        public virtual Invoice Invoice { get; set; }
        public virtual string Notes { get; set; }
        public virtual int CreditNoteRefNumber { get; set; }
    }
}
