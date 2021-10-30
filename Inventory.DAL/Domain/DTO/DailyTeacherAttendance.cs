using System;

namespace AECMIS.DAL.Domain.DTO
{
    public class DailyTeacherAttendance
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public double TotalTime { get; set; }
        public int TotalLessons { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime Date { get; set; }
        public int AttendanceId { get; set; }

        public virtual int ExtraTimeInMins { get; set; }
        public virtual string ExtraWork { get; set; }

        public virtual int? TeacherCoveredFor { get; set; }
        public virtual decimal Paid { get; set; }

    }
}
