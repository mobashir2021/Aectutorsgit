using System;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class SearchInvoiceDto
    {
        public string DateGeneratedFrom { get; set; }
        public string DateGeneratedTo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentNo { get; set; }
        public InvoiceStatus? InvoiceStatus { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
