namespace AECMIS.DAL.Domain.DTO
{
    public class ReceiptViewModel
    {
        public int Id { get; set; }
        public int NumberOfSessionsPaidFor { get; set; }
        public string AmountPaid { get; set; }
        public string PaymentDate { get; set; }
        public string PaymentType { get; set; }
        public string ChequeNo { get; set; }

    }
}