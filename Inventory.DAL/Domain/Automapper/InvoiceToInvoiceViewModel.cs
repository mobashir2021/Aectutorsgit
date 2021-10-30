using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Extensions;
using AutoMapper;

namespace AECMIS.DAL.Domain.Automapper
{
    public class InvoiceToInvoiceViewModel : ITypeConverter<Invoice, InvoiceViewModel>
    {
        public InvoiceViewModel Convert(Invoice source, InvoiceViewModel destination, ResolutionContext context)
        {
            var invoiceViewModel = new InvoiceViewModel
            {
                Name = source.Student.FirstName + " " + source.Student.LastName,
                InvoiceNo = source.InvoiceRefNumber,
                StudentNo = source.Student.StudentNo,
                Status = source.Status.ToString(),
                DateOfGeneration = source.DateOfGeneration.Localize().ToString("dd-MM-yyyy"),
                TotalAfterDiscount = source.TotalAfterDiscount.ToString("C"),
                Discount = source.DiscountApplied.HasValue ? source.DiscountApplied.GetValueOrDefault().ToString("C") : "0:00",
                Total = source.TotalAmount.ToString("C"),
                InvoiceType = source.PaymentType.ToString(),
                Id = source.Id,
                SessionCredits = source.NumberOfSessionsPayingFor

            };

            if (source.PaymentReciept != null)
            {
                invoiceViewModel.Reciept = new ReceiptViewModel
                {
                    Id = source.PaymentReciept.Id,
                    //AmountPaid = source.PaymentReciept.InvoicePayment.PaymentAmount.GetValueOrDefault().ToString("C"),
                    AmountPaid = source.TotalAfterDiscount.ToString("C"),
                    ChequeNo = source.PaymentReciept.InvoicePayment.ChequeNo,
                    NumberOfSessionsPaidFor = source.PaymentReciept.InvoicePayment.NumberOfSessionsPaidFor,
                    PaymentDate = source.PaymentReciept.InvoicePayment.PaymentDate.GetValueOrDefault().ToString("dd-MM-yyyy"),
                    PaymentType = source.PaymentReciept.InvoicePayment.PaymentType.ToString()
                };
            }

            return invoiceViewModel; 
        }
    }
}