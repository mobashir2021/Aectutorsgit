using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.DTO;
using AutoMapper;

namespace AECMIS.DAL.Domain.Automapper
{
    public class StudentsToStudentSearchResultRow : ITypeConverter<List<Student>, List<StudentSearchResultRow>>
    {
        public List<StudentSearchResultRow> Convert(List<Student> source, List<StudentSearchResultRow> destination, ResolutionContext context)
        {
            return source.ToList().Select(
                x =>
                new StudentSearchResultRow
                {
                    StudentId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Curriculum = x.Curriculum.ToString(),
                    StudentNo = x.StudentNo
                })
                .ToList();

        }
    }
}