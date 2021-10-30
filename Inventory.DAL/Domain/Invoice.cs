using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.DAL.Validation;
using FluentValidation.Results;

namespace AECMIS.DAL.Domain
{
    public class Invoice:Entity
    {
        public Invoice()
        {
            DebitedSessions = new List<SessionAttendance>();
        }

        public virtual Student Student { get; set; }        
        public virtual decimal TotalAmount { get; set; }
        public virtual decimal? DiscountApplied { get; set; }
        public virtual decimal TotalAfterDiscount { get; set; }
        public virtual int InvoiceRefNumber { get; set; }
        public virtual decimal VATAmount { get; set; }
        public virtual decimal TotalExcludeVAT { get; set; }
        public virtual int NumberOfSessionsPayingFor { get; set; }
        public virtual DateTime DateOfGeneration { get; set; }
        public virtual InvoiceStatus Status { get; set; }
        public virtual InvoicePaymentType PaymentType { get; set; }
        public virtual IList<SessionAttendance> DebitedSessions { get; set; }
        public virtual PaymentReciept PaymentReciept { get; set; }
        
        //Changes added
        private readonly Func<PaymentReciept, bool> _paymentRecieptCreditsAreAvailable =
          (reciept) => reciept.AttendanceCredits != null && reciept.AttendanceCredits.Count(x => x.Attendance == null) > 0;

        public virtual bool ProcessPayment(InvoicePayment payment)
        {
            IsPayable();
            
            //create payment object e.t.c.
            PaymentReciept = new PaymentReciept
            {
                Invoice = this,
                GeneratedDate = payment.PaymentDate.GetValueOrDefault(),
                InvoicePayment = payment
            };

            
            Status = InvoiceStatus.Paid;
            DiscountApplied = 0;
            PaymentReciept.IsValid();
            //TotalAmount = payment.PaymentAmount.GetValueOrDefault();
            //TotalAfterDiscount = payment.PaymentAmount.GetValueOrDefault() - DiscountApplied.GetValueOrDefault();
            //NumberOfSessionsPayingFor = payment.NumberOfSessionsPaidFor;
            

            return true;
        }

        /// <summary>
        /// Create a paid invoice
        /// </summary>
        /// <returns></returns>
        public virtual bool ProcessPayment(PaymentPlan actualPaymentPlan, PaymentPlan defaultPaymentPlan, InvoicePayment payment, decimal discountApplied)
        {
            IsPayable();  
          


            //create payment object e.t.c.
            PaymentReciept = new PaymentReciept
                                 {
                                     Invoice = this,
                                     GeneratedDate = payment.PaymentDate.GetValueOrDefault(),
                                     InvoicePayment = payment
        };
            
            Status = InvoiceStatus.Paid;
            //onyly apply discount when the amount paid is the same as price plan when discount was agreed
            DiscountApplied = payment.PaymentAmountId != defaultPaymentPlan.Id ? 0 : discountApplied;

            TotalAmount = actualPaymentPlan.Amount;
            TotalAfterDiscount = actualPaymentPlan.Amount - DiscountApplied.GetValueOrDefault();
            NumberOfSessionsPayingFor = payment.NumberOfSessionsPaidFor;

            PaymentReciept.IsValid();

            return true;
        }


        private void IsPayable()
        {
           new InvoiceIsPayable().ValidateAndThrow(this);
        }

        private ValidationResult IsConvertibleToDaily()
        {
            return new InvoiceIsConvertibleToDaily().Validate(this);
        }



        public virtual void ConvertUnpaidFutureInvoiceToDailyRateInvoice(List<PaymentPlan> paymentPlans)
        {
            var invoiceValidationResult = IsConvertibleToDaily();
            if (!invoiceValidationResult.IsValid) return;

            var dailyRate =
                paymentPlans.FirstOrDefault(
                    x => x.Curriculum == Student.DefaultPaymentPlan.Curriculum && x.TotalSessions == 1);

            if (dailyRate == null)
                throw new Exception(string.Format("Could not find daily rate for curriculum {0}",
                                                  Student.DefaultPaymentPlan.Curriculum));

            PaymentType = InvoicePaymentType.Daily;
            TotalAmount = dailyRate.Amount;
            TotalAfterDiscount = dailyRate.Amount; //no discount for daily rate
            DiscountApplied = 0;
            NumberOfSessionsPayingFor = dailyRate.TotalSessions;

        }


        public virtual void DebitInvoiceWithAttendences(List<SessionAttendance> sessionAttendances)
        {
            if (PaymentType != InvoicePaymentType.Advanced)
                throw new Exception("Only advance payments can be debited for attendences");

            if (PaymentReciept == null) throw new Exception("Invoice must be paid before debiting accounts");

            //while payment.attendances assigned is less than the number of session paid for add attendance
            //TODO:Fix below
            while (DebitedSessions.Count(s => s.Status != SessionAttendanceStatus.AuthorizeAbsence) < PaymentReciept.InvoicePayment.NumberOfSessionsPaidFor)
            {
                //get next attendance 
                var sessionAttendance = sessionAttendances.OrderByDescending(x => x.SessionRegister.Date).
                    FirstOrDefault(x => x.Invoice == null);
                if (sessionAttendance == null) break;

                sessionAttendance.ValidateIsDebitableAndThrow();
                sessionAttendance.Invoice = this;
                //Changes added
                //sessionAttendance.RemainingCredits = PaymentReciept.InvoicePayment.NumberOfSessionsPaidFor - DebitedSessions.Count;
                //Update the session attendance in Credit table
                DebitAttendanceCreditWithAttendences(PaymentReciept, sessionAttendance);
                //Associate a credit with this attendance               
                //logic to debit credit
                DebitedSessions.Add(sessionAttendance);                
            }

            //if there is only 1 credit remaining throw notification to generate advanced invoice
            //if(DebitedSessions.Count == Student.DefaultPaymentPlan.TotalSessions-1)

        }
        //Changes added
        public virtual void DebitAttendanceCreditWithAttendences(PaymentReciept paymentReciept, SessionAttendance sessionAttendance)
        {
            if (_paymentRecieptCreditsAreAvailable(paymentReciept))
            {
                var attendanceCredit = PaymentReciept.AttendanceCredits.OrderByDescending(x => x.Id).
                  FirstOrDefault(x => x.Attendance == null);
                if (attendanceCredit == null) return;

                attendanceCredit.Attendance = sessionAttendance;
            }
        }
}
}
