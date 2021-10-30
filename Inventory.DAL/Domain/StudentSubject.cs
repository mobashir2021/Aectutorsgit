using System;

namespace AECMIS.DAL.Domain
{
    public class StudentSubject : Entity
    {
        private StudentSubjectId _studentSubjectId = new StudentSubjectId();

        private Session _session;
        private Student _student;
        private Subject _subject;

        public virtual DateTime Version { get; set; }
        public virtual StudentSubjectId StudentSubjectId { get { return _studentSubjectId; } set { _studentSubjectId = value; } }
        public virtual Session Session { get { return _session; } set { _session = value; _studentSubjectId.SessionId = _session.Id; } }
        public virtual Student Student { get { return _student; } set { _student = value; _studentSubjectId.StudentId = _student.Id; } }
        public virtual Subject Subject { get { return _subject; } set { _subject = value; _studentSubjectId.SubjectId = _subject.Id; } }
    }
}
