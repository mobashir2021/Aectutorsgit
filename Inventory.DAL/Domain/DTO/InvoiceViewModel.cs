using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public string DateOfGeneration { get; set; }
        public string Status { get; set; }
        public string StudentNo { get; set; }
        public string Name { get; set; }
        public string Total { get; set; }
        public string Discount { get; set; }
        public string TotalAfterDiscount { get; set; }
        public string VATAmount { get; set; }
        public string InvoiceType { get; set; }
        public int SessionCredits { get; set; }
        public ReceiptViewModel Reciept { get; set; }
        public string VATLabel { get; set; }
        public string LogoPath { get; set; }
        public string Gross { get; set; }
        public int InvoiceNo { get; set; }
        public string NameOfStudent { get; set; }
        public string TotalExcludeVAT { get; set; }
        public string PaymentDate { get; set; }
        public string StatusInvoice { get; set; }
        public string CreditNotes { get; set; }
        public string NetAmount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string CreditReferenceNo { get; set; }
    }

    public class InvalidateInvoiceViewModel
    {
        public int Id { get; set; }
        public List<StudentAttendanceDetails> listStudentAttendanceDetails { get; set; }
        public string SaveUrl { get; set; }
        public string CreditNote { get; set; }
        public int PaymentPlanSelectedInv { get; set; }
        public int PaymentTypeSelectedInv { get; set; }
        public string ChequeNoInv { get; set; }
        public System.DateTime PaymentDateInv { get; set; }
        public int SessionAttendanceId { get; set; }
        public bool IsInvalidateInvoiceEnabled { get; set; }

        public bool IsFutureInvoiceAvailable { get; set; }


    }
}