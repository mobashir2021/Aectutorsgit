using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class InvoicePaymentMap : BaseMap<InvoicePayment>
    {
        public InvoicePaymentMap()
        {
            //Changes added - Table name renamed
            Table("payment.Payment");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.NumberOfSessionsPaidFor).Column("NumberOfSessionsPaidFor");
            Map(x => x.PaymentAmount).Column("AmountPaid");
            Map(x => x.PaymentAmountId).Column("PaymentId");
            Map(x => x.PaymentDate).Column("PaymentDate");
            //Map(x => x.SessionsInDebit).Column("SessionsInDebit").Not.Nullable();
            Map(x => x.PaymentType).Column("PaymentType").Nullable();
            Map(x => x.ChequeNo).Column("ChequeNo");
            References(x => x.Attendance).Column("AttendanceId").Nullable();
            References(x => x.Invoice).Column("InvoiceId").Nullable();
        }
    }
}