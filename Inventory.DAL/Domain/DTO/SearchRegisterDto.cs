using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class SearchRegisterDto
    {
        public SessionRegisterStatus? Status { get; set; }
        public int? CentreId { get; set; }
        public int? SessionId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int PageIndex { get;set; }
        public int PageSize { get; set; }
    }
}
