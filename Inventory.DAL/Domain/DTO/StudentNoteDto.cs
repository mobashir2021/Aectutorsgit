using System;

namespace AECMIS.DAL.Domain.DTO
{
    public class StudentNoteDto
    {
        public int StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}
