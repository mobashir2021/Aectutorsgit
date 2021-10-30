using AECMIS.DAL.Domain;
using FluentValidation;

namespace AECMIS.DAL.Validation
{
    public class InvoiceIsPayable : AbstractValidator<Invoice>
    {
        public InvoiceIsPayable()
        {
            RuleFor(x => x.PaymentReciept).Equals(null);
            //RuleFor(x => x.PaymentReciept.AmountPaid).Equal(InvoicePaymentType.Advanced).WithMessage("Invoice must be an");
        }
    }
}