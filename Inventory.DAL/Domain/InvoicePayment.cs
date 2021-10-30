using System;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Validation;

namespace AECMIS.DAL.Domain
{
    public class InvoicePayment : Entity
    {
        public virtual SessionAttendance Attendance { get; set; }
        public virtual decimal? PaymentAmount { get; set; }
        public virtual int? PaymentAmountId { get; set; }
        public virtual PaymentType? PaymentType { get; set; }
        public virtual string ChequeNo { get; set; }
        public virtual int NumberOfSessionsPaidFor { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual PaymentReciept Reciept { get; set; }
        public virtual DateTime? PaymentDate { get; set; }

        
    }
}
