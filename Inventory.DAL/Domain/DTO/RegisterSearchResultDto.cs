using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class RegisterSearchResultDto
    {
        public List<RegisterSearchResultRow> Registers { get; set; }
        public string SearchRegistersUrl { get; set; }
        public string AddRegisterUrl { get; set; }
        public int RowCount { get; set; }
        public int MaxPageIndex { get; set; }
        public int PageSize { get; set; }
        public bool CanAddNewRegister { get; set; }
    }
}
