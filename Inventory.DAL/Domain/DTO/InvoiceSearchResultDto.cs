using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class InvoiceSearchResultDto
    {
        public List<InvoiceSearchResultRow> Data { get; set; }
        public string DoSearchUrl { get; set; }
        public int RowCount { get; set; }
        public int MaxPageIndex { get; set; }
        public int PageSize { get; set; }
        public string DateGeneratedFrom { get; set; }
        public string DateGeneratedTo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentNo { get; set; }

        public StudentSearchResultDto StudentSearchViewModel { get; set; }
    }
}
