namespace AECMIS.DAL.Domain
{
    public class StudentSubjectId
    {
        public virtual int SessionId { get; set; }
        public virtual int StudentId { get; set; }
        public virtual int SubjectId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var t = obj as StudentSubjectId;
            if (t == null)
                return false;
            if (SessionId == t.SessionId && StudentId == t.StudentId
                && SubjectId == t.SubjectId)
                return true;
            return false;

        }


        public override int GetHashCode()
        {
            return (SessionId + "|" + StudentId + "|" + SubjectId).GetHashCode();
        }
    }
}