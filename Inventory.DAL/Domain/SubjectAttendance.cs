namespace AECMIS.DAL.Domain
{
    public class SubjectAttendance:Entity
    {
        public virtual SessionAttendance Attendance { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual string Notes { get; set; }
    }
}