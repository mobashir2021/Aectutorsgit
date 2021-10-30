using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class StudentSearchResultDto
    {
        public StudentSearchResultDto()
        {
            Students =new List<StudentSearchResultRow>();
        }

        public List<StudentSearchResultRow> Students { get; set; }
        public string SearchStudentsUrl { get; set; }
        public string AddStudentUrl { get; set; }
        public int RowCount { get; set; }
        public int MaxPageIndex { get; set; }
        public int PageSize { get; set; }
        public bool AreStudentsSelectable { get; set; }
        public bool CanAddNewStudent { get; set; }
    }
}