using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.Service.DTO;
using AutoMapper;
using System.Linq;
namespace AECMIS.MVC.AutoMapper
{
    public class PaymentPlanToPaymentPlanModel : ITypeConverter<List<PaymentPlan>,List<PaymentPlanViewModel>>
    {
        

        public List<PaymentPlanViewModel> Convert(List<PaymentPlan> source, List<PaymentPlanViewModel> destination, ResolutionContext context)
        {
            return source.Select(x => new PaymentPlanViewModel
            {
                PaymentPlanDisplay =
                                                 string.Format("{0} Sessions -({1}->{2})", x.TotalSessions,
                                                               x.Curriculum, x.Amount.ToString("C")),
                PaymentPlanId = x.Id,
                Curriculum = x.Curriculum,
                TotalNumberOfSessions = x.TotalSessions

            }).ToList();
        }
    }
}