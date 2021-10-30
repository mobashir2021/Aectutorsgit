using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain.DTO
{
    public class TeacherAttendanceDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<DailyTeacherAttendance> DailyTeacherAttendances { get; set; }
    }
}
