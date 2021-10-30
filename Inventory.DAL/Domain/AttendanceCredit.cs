using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain
{
    public class AttendanceCredit : Entity
    {
        public virtual SessionAttendance Attendance { get; set; }
        public virtual PaymentReciept Receipt { get; set; }
    }
}
