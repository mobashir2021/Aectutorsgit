using AECMIS.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class AttendanceCreditMap : BaseMap<AttendanceCredit>
    {
        public AttendanceCreditMap()
        {
            Table("timetable.AttendanceCredit");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            References(x => x.Receipt).Column("InvoiceReceiptId");
            References(x => x.Attendance).Column("AttendanceId");
        }
    }
}