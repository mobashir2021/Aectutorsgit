using AECMIS.DAL.Domain;
using FluentNHibernate.Mapping;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class PaymentPlanMapping:BaseMap<PaymentPlan>
    {
        public PaymentPlanMapping()
        {
            Table("payment.PaymentPlan");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.Curriculum).Column("Curriculum");
            Map(x => x.Amount).Column("Amount");
            Map(x => x.TotalSessions).Column("TotalSessions");
        }
    }
}
