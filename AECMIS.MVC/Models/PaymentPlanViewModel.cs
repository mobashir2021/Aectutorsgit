using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.Service.DTO
{
    public class PaymentPlanViewModel
    {
        public string PaymentPlanDisplay { get; set; }
        public int PaymentPlanId { get; set; }
        public Curriculum Curriculum { get; set; }
        public int TotalNumberOfSessions { get; set; }
    }
}