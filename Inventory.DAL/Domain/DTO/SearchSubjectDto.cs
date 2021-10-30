using AECMIS.DAL.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain.DTO
{
    public class SearchSubjectDto
    {
        public int? SubjectId { get; set; }
        public Curriculum? Level { get; set; }
        public string Name { get; set; }
    }
}
