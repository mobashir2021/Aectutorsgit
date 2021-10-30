
namespace AECMIS.DAL.Domain.DTO
{
    public class InvoiceSearchResultRow:InvoiceViewModel
    {
        public string ViewInvoiceUrl { get; set; }
        public string DownloadInvoiceUrl { get; set; }
        public string InvalidateInvoiceUrl { get; set; }
        public string EditInvoiceUrl { get; set; }
        public bool IsPaymentDone { get; set; }
        public int currentInvoiceId { get; set; }
        public bool IsProcessed { get; set; }
        public string VerifyAdminPasswordUrl { get; set; }

        public string UpdatePaymentUrl { get; set; }

        public string MakePaymentDialogDataUrl { get; set; }
        
    }
}
