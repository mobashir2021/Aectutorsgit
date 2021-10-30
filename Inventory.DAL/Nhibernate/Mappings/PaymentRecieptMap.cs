using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class PaymentRecieptMap : BaseMap<PaymentReciept>
    {
        public PaymentRecieptMap()
        {
            Table("payment.InvoiceReciept");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.GeneratedDate).Column("GeneratedDate");
            References(x => x.InvoicePayment).Column("PaymentId");
            References(x => x.Invoice).Column("InvoiceId");
            HasMany(x => x.AttendanceCredits).KeyColumn("InvoiceReceiptId").Cascade.All().LazyLoad();
        }
    }
}