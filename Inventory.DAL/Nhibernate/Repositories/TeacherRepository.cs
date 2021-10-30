using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public class TeacherRepository:Repository<Teacher,int>
    {
        private readonly ISession _session;
        public TeacherRepository():this(null)
        {
        }

        public TeacherRepository(ISession session)
        {
            _session = session ?? SessionManager.GetSession<Teacher>();
        }

        protected override ISession Session
        {
            get { return _session; }
        }

        public List<Teacher> GetTeachersByCentre(int centreId)
        {
            return (from sr in Session.Query<SessionRegister>()
                     join sa in Session.Query<SessionAttendance>() on sr.Id equals sa.SessionRegister.Id
                     join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
                     join session in Session.Query<Session>() on sr.Session.Id equals session.Id
                     where session.Location.Id == centreId
                     select sas.Teacher).Distinct().ToList();
        }

        public  List<Teacher> GetTeachersByStudent(int studentId)
        {
            return (from sa in Session.Query<SessionAttendance>()
                    join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
                    where sa.Student.Id == studentId
                    select sas.Teacher).Distinct().ToList();
        }


        public List<TeacherSessionAttendance> GetTeacherSessionAttendances(DateTime fromDate, DateTime toDate)
        {
            return (from sr in Session.Query<SessionRegister>()
                    join sa in Session.Query<SessionAttendance>() on sr.Id equals sa.SessionRegister.Id
                    join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
                    join session in Session.Query<Session>() on sr.Session.Id equals session.Id
                    join teacher in Session.Query<Teacher>() on sas.Teacher.Id equals teacher.Id
                    where sr.Date >= fromDate && sr.Date <= toDate
                    group sa by new {sr.Session, sas.Teacher, sr.Date}
                    into grp
                    select grp.Key).ToList().
                Select(x =>
                       new TeacherSessionAttendance
                           {Session = x.Session, AttendanceDateTime = x.Date, Teacher = x.Teacher})
                .ToList();

        }
    }

    public class TeacherSessionAttendance
    {
        public Session Session { get; set; }
        public DateTime AttendanceDateTime { get; set; }
        public Teacher Teacher { get; set; }
    }
}
