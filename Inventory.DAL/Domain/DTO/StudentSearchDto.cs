using AECMIS.DAL.Domain.Enumerations;
using System;

namespace AECMIS.DAL.Domain.DTO
{
    public class StudentSearchDto
    {
        public string StudentNo { get; set; }
        public int Centre { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Curriculum? Curriculum { get; set; }
        public bool ActiveOnly { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
