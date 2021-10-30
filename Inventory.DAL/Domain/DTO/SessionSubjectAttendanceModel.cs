using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class SessionSubjectAttendanceModel
    {
        public int? AttendanceId { get; set; }
        public int StudentId { get; set; }
        public SessionAttendanceStatus? Status { get; set; }
        //public int SessionId { get; set; }
        //public DateTime Date { get; set; }
        public int? SubjectId { get; set; }
        public int? TeacherId { get; set; }
        public string Notes { get; set; }
        public string PreviousNotes { get; set; }

        public bool PaymentRequired { get; set; }
        public List<PaymentViewModel> OutstandingPayments { get; set; }

        public PaymentViewModel FutureInvoicePayment { get; set; }



    }

    public class PaymentViewModel
    {
        public int InvoiceId { get; set; }
        public PaymentType? PaymentType { get; set; }
        public string ChequeNo { get; set; }
        public int? PaymentAmount { get; set; }
        public bool IsFutureInvoice { get; set; }
        public int PaymentRequiredPlanId { get; set; }
        public string PaymentDate { get; set; }
        public List<DiscountedPaymentAmount> PaymentPlans { get; set; }
    }
}