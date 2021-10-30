using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Validation;
using System.Linq;
namespace AECMIS.DAL.Domain
{
    public class SessionAttendance:Entity
    {

        public SessionAttendance()
        {
            SubjectsAttended = new List<SubjectAttendance>();
            Payments = new List<InvoicePayment>();
        }

        public virtual Student Student { get; set; }
        public virtual SessionAttendanceStatus? Status { get; set; }
        public virtual IList<SubjectAttendance> SubjectsAttended { get; set; }
        public virtual SessionRegister SessionRegister { get; set; }
        //Changes added -- Removed column
        //public virtual int RemainingCredits { get; set; }

        //this is only set when an advanced paid invoice has been debited or 
        //when a daily invoice has been created against the attendance  
        public virtual Invoice Invoice { get; set; }
        public virtual IList<InvoicePayment> Payments { get; set; }
        
        //Changes added
        //public virtual InvoicePayment PaymentForFutureInvoice
        //{
        //    get
        //    {
        //        //If payments are available then check for invoices. If invoice does not present against a payment then check for number of sessions paid for
        //        if (Payments == null) return null;

        //        //should we not assume a future invoice payment is one that does not have an invoice against it??
        //        return Payments.First(x => x.Invoice == null);
        //        //if (Payments.Count(x => x.Invoice != null) > 0 &&
        //        //    Payments.Any(x => x.Invoice.PaymentType == InvoicePaymentType.Advanced))
        //        //    return Payments.First(x => x.Invoice.PaymentType == InvoicePaymentType.Advanced);
        //        //else
        //        //    return Payments.First(x => x.NumberOfSessionsPaidFor > 1);
        //    }

        //}


        public virtual void ValidateIsDebitableAndThrow()
        {
            new SessionAttendanceDebitValidator().ValidateAndThrow(this);
        }

        public virtual  bool IsValidForPersistance()
        {
            throw new NotImplementedException();
        }

        public static SessionAttendance CreateAttendance(Session session, DateTime date,Student student, SessionRegister sessionRegister, SessionSubjectAttendanceModel model)
        {
            return new SessionAttendance
            {
                //TODO
                //PaymentType = model.PaymentType,
                //ChequeNo = model.ChequeNo,
                //PaymentAmount = model.PaymentAmount,                
                Status = model.Status,
                Student = student,
                SessionRegister = sessionRegister
            };
        }
    }
}
