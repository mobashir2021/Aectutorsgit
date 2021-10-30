using System;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using FluentValidation;

namespace AECMIS.DAL.Validation
{
    public class InvoicePaymentRecieptValidator : AbstractValidator<PaymentReciept>
    {
        public InvoicePaymentRecieptValidator()
        {
            RuleFor(x => x.InvoicePayment.PaymentAmount).Equal(x => x.Invoice.TotalAfterDiscount).When(
                x => x.Invoice.PaymentType == InvoicePaymentType.Daily).WithMessage("Daily invoice must be paid in exact amount");
            RuleFor(x => x.InvoicePayment.NumberOfSessionsPaidFor).Equal(1).When(x => x.Invoice.PaymentType == InvoicePaymentType.Daily);
            RuleFor(x => x.InvoicePayment.ChequeNo).NotNull().NotEmpty().When(x => x.InvoicePayment.PaymentType == PaymentType.Cheque);
            RuleFor(x => x.InvoicePayment.PaymentAmount).NotNull().When(
                x => x.InvoicePayment.Attendance.Status == SessionAttendanceStatus.Attended);
            RuleFor(x => x.InvoicePayment.PaymentDate).NotNull().When(
                x => x.InvoicePayment.PaymentType != PaymentType.None).WithMessage("Payment date required");
            RuleFor(x => x.InvoicePayment.PaymentAmountId).NotNull().When(
                x => x.InvoicePayment.PaymentType != PaymentType.None).WithMessage("Payment amount required");

        }
        
    }
}