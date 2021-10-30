using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AutoMapper;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public  class StudentRepository:Repository<Student,int>
    {
        private readonly ISession _session;
        public StudentRepository():this(null)
        {
        }

        public StudentRepository(ISession session)
        {
            _session = session ?? SessionManager.GetSession<Student>();
        }

        protected override ISession Session
        {
            get { return _session; }
        }

        public List<Student> GetStudentsWithNullCredits(StudentSearchDto studentSearch)
        {

            IList<int> ids = (from s in Session.Query<Student>()
                              join i in Session.Query<Invoice>() on s.Id equals i.Student.Id
                              join ir in Session.Query<PaymentReciept>() on i.PaymentReciept.Id equals ir.Id
                              join ac in Session.Query<AttendanceCredit>() on ir.Id equals ac.Receipt.Id
                              where ac.Attendance == null
                              group i by s.Id into grp
                              select grp.Key).ToList();

            var query = from s in Session.Query<Student>()
                        select s;

            if (!string.IsNullOrEmpty(studentSearch.FirstName))
            {
                query = query.Where(x => x.FirstName == studentSearch.FirstName);
            }

            if (!string.IsNullOrEmpty(studentSearch.LastName))
            {
                query = query.Where(x => x.FirstName == studentSearch.LastName);
            }

            if (!string.IsNullOrEmpty(studentSearch.StudentNo))
            {
                query = query.Where(x => x.StudentNo == studentSearch.StudentNo);
            }

            if (studentSearch.Curriculum != null)
            {
                query = query.Where(x => x.Curriculum == studentSearch.Curriculum.GetValueOrDefault());
            }

            if (studentSearch.ActiveOnly)
            {
                query = query.Where(x => x.Enabled);
            }
            return query.Where(x => !ids.Contains(x.Id)).ToList();
        }

        public StudentSearchResultDto SearchStudents(StudentSearchDto studentSearch)
        {
            var query = Session.QueryOver<Student>();


            if (studentSearch.ActiveOnly)
                query = query.Where(x => x.Enabled);

            if (!string.IsNullOrEmpty(studentSearch.FirstName))
            {
                query = query.WhereRestrictionOn(x => x.FirstName).IsLike(studentSearch.FirstName, MatchMode.Anywhere);
            }

            if (!string.IsNullOrEmpty(studentSearch.LastName))
            {
                query = query.WhereRestrictionOn(x => x.LastName).IsLike(studentSearch.LastName, MatchMode.Anywhere);
            }

            if (!string.IsNullOrEmpty(studentSearch.StudentNo))
            {
                query = query.WhereRestrictionOn(x => x.StudentNo).IsLike(studentSearch.StudentNo, MatchMode.Anywhere);
            }

            if (studentSearch.Curriculum != null)
            {
                query = query.Where(x => x.Curriculum == studentSearch.Curriculum.GetValueOrDefault());
            }

            if (studentSearch.DateOfBirth != null)
            {
                query = query.Where(x => x.DateOfBirth == studentSearch.DateOfBirth.GetValueOrDefault());

            }


            IList<Student> list =
                query.OrderBy(x => x.StudentNo).Asc().Skip(studentSearch.PageIndex * studentSearch.PageSize)
                    .Take(studentSearch.PageSize).List();
            var rowCount = CriteriaTransformer.TransformToRowCount(query.UnderlyingCriteria)
                .UniqueResult<int>();


            return new StudentSearchResultDto
                       {
                           Students = Mapper.Map<List<StudentSearchResultRow>>(list),
                           MaxPageIndex = (int)Math.Ceiling((decimal)( (rowCount-1)/studentSearch.PageSize)),
                           PageSize = studentSearch.PageSize
                       };
        }
    }

    public class CreditCount
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }
}
