using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class RegisterSearchResultRow
    {
        public int RegisterId { get; set; }
        public string SessionInfo { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
        public int SessionId { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }
    }
}
