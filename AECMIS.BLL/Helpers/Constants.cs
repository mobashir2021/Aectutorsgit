using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.Service.Helpers
{
    public static class Constants
    {
        public const string INVALID_CENTRE_MESSAGE = "A session must be created against a valid centre";
        public const string DUPLICATE_SESSION_MESSAGE = "Session with these details already exists, it must be unique for a centre, day and time";
        public const string DUPLICATE_SUBJECT_MESSAGE = "Subject {0} in Curriculum {1} already exists please provide unique subject names";
        public const string DUPLICATE_TEACHER_MESSAGE = "Teacher with these details already exists, please ensure first, middle and last names are unique";
    }
}
