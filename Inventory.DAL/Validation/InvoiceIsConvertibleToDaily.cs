using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using FluentValidation;

namespace AECMIS.DAL.Validation
{
    public class InvoiceIsConvertibleToDaily : AbstractValidator<Invoice>
    {
        public InvoiceIsConvertibleToDaily()
        {
            RuleFor(x => x.PaymentType).NotEqual(InvoicePaymentType.Daily);
        }
    }
}