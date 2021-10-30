using System.Collections.Generic;
using AECMIS.DAL.Domain;
using FluentNHibernate;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class InvoiceMap : BaseMap<Invoice>
    {
        public InvoiceMap()
        {
            Table("payment.Invoice");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.DateOfGeneration).Column("DateOfGeneration").Not.Nullable();
            Map(x => x.DiscountApplied).Column("AppliedDiscount").Nullable();
            Map(x => x.InvoiceRefNumber).Column("InvoiceRefNumber").Nullable();
            Map(x => x.VATAmount).Column("VATAmount").Nullable();
            Map(x => x.TotalExcludeVAT).Column("TotalExcludeVAT").Nullable();
            Map(x => x.NumberOfSessionsPayingFor).Column("SessionCredit");
            Map(x => x.TotalAmount).Column("TotalAmount").Not.Nullable();
            Map(x => x.TotalAfterDiscount).Column("TotalAfterDiscounts").Not.Nullable();
            Map(x => x.PaymentType).Column("InvoiceType");
            Map(x => x.Status).Column("Status");
            References(x => x.Student).Column("StudentId").Not.Nullable();
            HasMany(x => x.DebitedSessions).KeyColumn("InvoiceId");
            HasOne(x => x.PaymentReciept).PropertyRef(r=> r.Invoice).Cascade.All();
        }
    }

    public class CreditNoteMap : BaseMap<CreditNote>
    {
        public CreditNoteMap()
        {
            Table("payment.CreditNote");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Notes).Column("Notes").Nullable();
            Map(x => x.CreditNoteRefNumber).Column("CreditNoteRefNumber").Nullable();
            References(x => x.Invoice).Column("InvoiceId").Not.Nullable();
        }
    }
}
