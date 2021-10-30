using AECMIS.DAL.Domain.DTO;
using AutoMapper;

namespace AECMIS.DAL.Domain.Automapper
{
    public class PaymentAmountToDiscountedPaymentAmount : ITypeConverter<PaymentPlan, DiscountedPaymentAmount>
    {
        public DiscountedPaymentAmount Convert(PaymentPlan source, DiscountedPaymentAmount destination, ResolutionContext context)
        {
            return new DiscountedPaymentAmount
            {
                Id = source.Id,
                PaymentAmountInCurrency = source.Amount.ToString("C"),
                PaymentAmount = source.Amount
            };
        }
    }
}