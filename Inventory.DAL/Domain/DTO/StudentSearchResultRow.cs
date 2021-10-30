namespace AECMIS.DAL.Domain.DTO
{
    public class StudentSearchResultRow
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Curriculum { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }

        public bool IsEditable { get; set; }
        public bool IsDeletable { get; set; }
    }
}