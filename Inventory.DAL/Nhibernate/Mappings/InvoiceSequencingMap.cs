using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AECMIS.DAL.Domain;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class InvoiceSequencingMap : BaseMap<InvoiceSequencing>
    {
        public InvoiceSequencingMap()
        {
            Table("payment.InvoiceSequencing");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.FiscalYear).Column("FiscalYear").Not.Nullable();
            Map(x => x.FromDate).Column("FromDate").Not.Nullable();
            Map(x => x.ToDate).Column("ToDate").Not.Nullable();
            Map(x => x.SequenceStartNum).Column("SequenceStartNum").Not.Nullable();
            Map(x => x.SequenceEndNum).Column("SequenceEndNum").Not.Nullable();
        }
    }

    public class CreditNoteSequencingMap : BaseMap<CreditNoteSequencing>
    {
        public CreditNoteSequencingMap()
        {
            Table("payment.CreditNoteSequencing");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.FiscalYear).Column("FiscalYear").Not.Nullable();
            Map(x => x.FromDate).Column("FromDate").Not.Nullable();
            Map(x => x.ToDate).Column("ToDate").Not.Nullable();
            Map(x => x.SequenceStartNum).Column("SequenceStartNum").Not.Nullable();
            Map(x => x.SequenceEndNum).Column("SequenceEndNum").Not.Nullable();
        }
    }
}
