using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.DTO;
using AutoMapper;
using System;

namespace AECMIS.DAL.Domain.Automapper
{
    public class InvoiceToInvoiceSearchResultRow : ITypeConverter<List<Invoice>, List<InvoiceSearchResultRow>>
    {
        public List<InvoiceSearchResultRow> Convert(List<Invoice> source, List<InvoiceSearchResultRow> destination, ResolutionContext context)
        {
            return source.Select(x => new InvoiceSearchResultRow
            {
                Gross = "\u00A3" + Math.Round((x.TotalAfterDiscount - x.VATAmount), 2),
                StatusInvoice = x.Status.ToString(),
                FirstName = x.Student.FirstName,
                LastName = x.Student.LastName,
                Discount = x.DiscountApplied.HasValue ? "\u00A3" + x.DiscountApplied.Value : "\u00A3" +  System.Convert.ToDecimal(0),
                NetAmount = x.TotalAfterDiscount.ToString(),
                PaymentDate = x.PaymentReciept!=null? x.PaymentReciept.InvoicePayment.PaymentDate.GetValueOrDefault().ToString("yyyy-MM-dd"):string.Empty,
                VATAmount = "\u00A3" + x.VATAmount,
                Name = x.Student.FirstName + " " + x.Student.LastName,
                StudentNo = x.Student.StudentNo,
                Status = x.Status.ToString(),
                DateOfGeneration = x.DateOfGeneration.ToString("yyyy-MM-dd"),
                TotalAfterDiscount = x.TotalAfterDiscount.ToString("C"),
                InvoiceType = x.PaymentType.ToString(),
                InvoiceNo = x.InvoiceRefNumber,
                Id = x.Id

            }).ToList();
        }
    }
}
