using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain
{
    public class PaymentPlan : Entity
    {
        public virtual int TotalSessions { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual Curriculum Curriculum { get; set; }
        public virtual bool IsDiscountedPlan { get; set; }


    }
}
