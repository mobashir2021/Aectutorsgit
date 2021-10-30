namespace AECMIS.DAL.Domain.DTO
{
    public class DiscountedPaymentAmount
    {
        public int Id { get; set; }
        public string PaymentAmountInCurrency { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}