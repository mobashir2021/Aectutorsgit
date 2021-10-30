using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.UnitTests.Helpers;
using FizzWare.NBuilder;
using NHibernate.Linq;

namespace AECMIS.DAL.UnitTests.Tests
{
    public static class DbData
    {

        internal static Domain.Session OnDay(this List<Domain.Session> sessions, DayOfWeek day)
        {
            return sessions.First(x => x.Day == day);
        }

        internal static List<Domain.Session> ManyOnDay(this List<Domain.Session> sessions, DayOfWeek day)
        {
            return sessions.Where(x => x.Day == day).ToList();
        }

        internal static Domain.Session Between(this List<Domain.Session> sessions, int from, int to )
        {
            return sessions.First(x => x.From.Hours == from && x.To.Hours == to);
        }

        internal static List<Domain.Session> AtCentre(this List<Domain.Session> sessions, int centreId)
        {
            return sessions.Where(x => x.Location.Id == centreId).ToList();
        }

        internal static List<Subject> WithName(this List<Subject> subjects,string name)
        {
            return subjects.
            Where(x => x.Name.ToLower() == name.ToLower()).ToList();
        }
        internal static Subject AtLevel(this List<Subject> subjects, Curriculum curr)
        {
            return subjects.First(x => x.Level == curr);
        }


        internal static Domain.Teacher WithId(this List<Domain.Teacher> teachers, int id)
        {
            return teachers.First(x => x.Id == id);
        }

        internal static Domain.Student WithId(this List<Domain.Student> students, int id)
        {
            return students.First(x => x.Id == id);
        }

        internal static Domain.Student WithAPendingAdvancedInvoice(this List<Domain.Student> students)
        {
            return students.First(x => x.Invoices.Count(
                            i => i.Status == InvoiceStatus.Pending && i.PaymentType == InvoicePaymentType.Advanced) > 0);
        }

        internal static DateTime LastMonday(this DateTime relativeDate)
        {
            return relativeDate.AddDays(DayOfWeek.Monday - relativeDate.DayOfWeek);
        }

        internal static Domain.Invoice LatestPendingAdvancedInvoice(this Domain.Student student)
        {
            return student.Invoices.First(i => i.Status == InvoiceStatus.Pending && i.PaymentType == InvoicePaymentType.Advanced);
        }

        internal static PaymentPlan PaymentPlanWithSessions(this Domain.Student student,MockDb db, int numberOfSessions)
        {
            return db.PaymentPlans.First(x => x.Curriculum == student.Curriculum && x.TotalSessions == numberOfSessions);
        }

        private static readonly Func<PaymentPlan, decimal, int, PaymentPlan> AddToPlan = (plan, arg2, arg3) =>
                                                                                             {
                                                                                                 plan.Amount = arg2;
                                                                                                 plan.TotalSessions =
                                                                                                     arg3;
                                                                                                 return plan;
                                                                                             };

        public static Domain.TuitionCentre GetDummyTuitionCentre()
        {
            return Builder<Domain.TuitionCentre>.CreateNew().Build();
        }


        public static Invoice GetDummyInvoice(Domain.Student student, decimal totalAmount, decimal discountApplied, int numberOfSessions)
        {
            return new Invoice
                              {
                                  TotalAmount = totalAmount,
                                  DiscountApplied = discountApplied,
                                  TotalAfterDiscount = totalAmount - discountApplied,
                                  NumberOfSessionsPayingFor = numberOfSessions,
                                  DateOfGeneration = DateTime.Today,
                                  Status = InvoiceStatus.Pending,
                                  Student = student,
                                  PaymentType = InvoicePaymentType.Advanced
                              };

        }


        public static SessionAttendance GetDummyAttendance(Domain.Student student, Subject subject, SessionAttendanceStatus status, Domain.Teacher teacher, Domain.Session session, Invoice invoice = null)
        {
            var attendance = new SessionAttendance
                       {
                           Student = student,
                           //Date = DateTime.Now.Date,
                           Invoice = invoice,
                           Status = status,
                           //Session = session
                       };

            attendance.SubjectsAttended.Add(new SubjectAttendance
            {
                Attendance = attendance,
                Subject = subject,
                Teacher = teacher,
            });

            return attendance;
        }

        static readonly Func<List<StudentSubject>, int, Subject> GetSubject = (list, id) => list.First(x => x.Subject.Id == id).Subject;

        static readonly Func<List<StudentSubject>, int, Domain.Session> GetSessionWithId =
            (list, id) => list.First(x => x.Session.Id == id).Session;

        public static Domain.Student GetDummyStudentWithAttendance(List<StudentSubject> subjects, int subjectId, int sessionId, PaymentPlan paymentPlan, Domain.Teacher teacher, decimal totalAmount, decimal discountApplied, int numberOfSessions, Domain.SessionRegister sessionRegister)
        {            
            var student = GetDummyStudent("Test", "Test", subjects, paymentPlan);
            var attendance = GetDummyAttendance(student, GetSubject(subjects, subjectId), SessionAttendanceStatus.Attended, teacher, GetSessionWithId(subjects, sessionId));
            attendance.SessionRegister = sessionRegister;
            student.SessionAttendances.Add(attendance);

            Persist(student);
            return student;
        }

        public static Domain.Student GetDummyStudentWithInvoice(List<StudentSubject> subjects, PaymentPlan paymentPlan, decimal totalAmount, decimal discountApplied, int numberOfSessions)
        {            
            var student = GetDummyStudent("Test", "Test", subjects, paymentPlan);
            var invoice = GetDummyInvoice(student, totalAmount, discountApplied, numberOfSessions);
            student.Invoices.Add(invoice);

            Persist(student);
            return student;
        }

        public static Domain.Student GetDummyStudent(string firstName, string lastName, List<StudentSubject> subjects, PaymentPlan paymentPlan)
        {

            var student = Builder<Domain.Student>.CreateNew().With(x => x.Id = 0).With(x=> x.Enabled = true).Build();
            student.Contacts =
                Builder<ContactPerson>.CreateListOfSize(1).All().With(x => x.Id = 0).With(
                    x => x.Student = student).Build();
            student.DefaultPaymentPlan = paymentPlan;
            student.EducationInstitutes = Builder<EducationInstitute>.CreateListOfSize(1).All().With(x => x.Id = 0).And(
                    x => x.Student = student).Build();

            subjects.ForEach(s => { s.Student = student; student.Subjects.Add(s); });
            Persist(student);
            
            return student;
        }

        public static List<Domain.TuitionCentre> PopulateTuitionCentres()
        {
            BuilderSetup.SetCreatePersistenceMethod<IList<Domain.TuitionCentre>>(Persist);
            return Builder<Domain.TuitionCentre>.CreateListOfSize(3).All().
                With(x => x.Id = 0).
                Persist().ToList();
        }

        public static Domain.Session GetSession(Domain.TuitionCentre location, TimeSpan from, TimeSpan to, DayOfWeek day = DayOfWeek.Thursday)
        {
            return new Domain.Session()
                       {
                           Location = location,
                           From = @from,
                           To = to,
                           Day = day,
                           SubjectsTaughtAtSession = new List<Subject>()
                       };
        }

        public static List<Domain.Session> PopulateSessions(Domain.TuitionCentre centre)
        {
            var sessions = new List<Domain.Session>
                               {
                                   GetSession(centre, new TimeSpan(11, 00, 00),
                                              new TimeSpan(13, 00, 00),
                                              DayOfWeek.Monday),
                                   GetSession(centre, new TimeSpan(13, 00, 00),
                                              new TimeSpan(15, 00, 00),
                                              DayOfWeek.Monday),
                                   GetSession(centre, new TimeSpan(11, 00, 00),
                                              new TimeSpan(13, 00, 00),
                                              DayOfWeek.Tuesday),
                                              GetSession(centre,new TimeSpan(13, 00, 00),
                                              new TimeSpan(15, 00, 00),
                                              DayOfWeek.Tuesday),
                                   GetSession(centre, new TimeSpan(11, 00, 00),
                                              new TimeSpan(13, 00, 00),
                                              DayOfWeek.Wednesday),
                               };

            Persist<Domain.Session>(sessions);
            return sessions;
        }

        public static Domain.Session PopulateSession(Domain.TuitionCentre centre)
        {
            return PopulateSessions(centre).First();
        }       
       
        public static List<Domain.Teacher> PopulateTeachers(int count)
        {
            BuilderSetup.SetCreatePersistenceMethod<IList<Domain.Teacher>>(Persist);
            return Builder<Domain.Teacher>.CreateListOfSize(count).All().With(x=> x.Id = 0 ).Persist().ToList();
        }

        public static Domain.Teacher PopulateTeacher()
        {
            return PopulateTeachers(2).First();
        }

        public static List<Subject> PopulateSubjects()
        {
            BuilderSetup.SetCreatePersistenceMethod<IList<Subject>>(Persist);
            //BuilderSetup.DisablePropertyNamingFor<Subject, int>(x => x.Id);

            return Builder<Subject>.CreateListOfSize(3).
                TheFirst(1).
                With(x => x.Name = "Maths").
                TheNext(1).
                With(x => x.Name = "Science").
                TheLast(1).
                With(x => x.Name = "English").
                All().
                With(x => x.Level = Curriculum.Gcse)
                .With(x=> x.Id =0).Persist().ToList();
        }


        public static void Persist<T>(IList<T> list) where T : Entity
        {
            var r = new SqLiteRepository<T, int>();
            list.ToList().ForEach(x => r.Save(x));
            //r.ClearSession();
        }

        public static void Persist<T>(T entity) where T : Entity
        {
            var r = new SqLiteRepository<T, int>();
            r.Save(entity);
            r.ClearSession();
        }
        

        public static PaymentPlan PopulatePaymentPlan()
        {
            return PopulatePaymentPlans().First();
        }


        public static List<PaymentPlan> PopulatePaymentPlans()
        {
            BuilderSetup.SetCreatePersistenceMethod<IList<PaymentPlan>>(Persist);

            return Builder<PaymentPlan>.CreateListOfSize(3).TheFirst(1).
                With(x => AddToPlan(x, 80, 4)).
                TheNext(1).
                With(x => AddToPlan(x, 24, 1)).
                TheNext(1).
                With(x => AddToPlan(x, 120, 6)).
                All().
                With(x => x.Curriculum = Curriculum.Gcse).And(x => x.Id = 0).
                Persist().ToList();
        }


        public static void PopulateSubjectsTaughtAtSessions(List<Subject> subjects, List<Domain.Session> sessions)
        {
            sessions.ForEach(x => subjects.ForEach(s => { x.SubjectsTaughtAtSession.Add(s); Persist(x); }));
        }

        public static IListBuilder<Domain.Student> PopulateStudents( MockDb db, Domain.Session session = null, PaymentPlan paymentPlan = null,int count = 1, List<Subject> subjects = null)
        {
            subjects = subjects ?? db.Subjects;
            session = session ?? db.Sessions.First(x => x.SubjectsTaughtAtSession.Count(s=> s.Level == Curriculum.Gcse && s.Name.ToLower() == "english") > 0);
            paymentPlan = paymentPlan ??
                          db.PaymentPlans.First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions != 1);  

            var students = Builder<Domain.Student>.CreateListOfSize(count).All()
                .With(x => x.Id = 0)
                .And(x => x.DefaultPaymentPlan = paymentPlan)
                .With(x => x.Contacts = Builder<ContactPerson>.CreateListOfSize(1).All().
                                            With(c => c.Id = 0).
                                            And(c => c.Student = x).Build())
                .With(x => x.EducationInstitutes = Builder<EducationInstitute>.CreateListOfSize(1).All().
                                                       With(e => e.Id = 0).
                                                       And(e => e.Student = x).
                                                       Build())
                .With(x => x.Curriculum = Curriculum.Gcse)
                .With(x => x.Subjects = Builder<StudentSubject>.CreateListOfSize(1).All()
                    .With(ss=> ss.Id = 0)
                    .With(ss=> ss.Session = session)
                    .With(ss=> ss.Subject = subjects.WithName("english").AtLevel(
                                                       paymentPlan.Curriculum) )
                    .With(ss => ss.Student = x)
                    .With(ss => ss.ModifiedBy = 0)
                    .With(ss=> ss.CreatedBy = 0 )                    
                    .With(ss=> ss.Version = DateTime.MinValue)
                    .Build()
                    ).With(x=> x.Invoices = Builder<Invoice>.CreateListOfSize(1).All()
                    .With(i=> i.Id =0)
                    .With(i=> i.PaymentType = InvoicePaymentType.Advanced)
                    .With(i=> i.Status = InvoiceStatus.Pending)
                    .With(i=> i.Student = x)
                    .With(i=> i.DateOfGeneration = DateTime.Today)
                    .With(i=> i.NumberOfSessionsPayingFor = paymentPlan.TotalSessions)
                    .With(i => i.TotalAfterDiscount = paymentPlan.Amount - x.DiscountAmount)
                    .Build());

            return students;
        }

        public static List<Domain.Student> PopulateAndPersistStudents(MockDb db, Domain.Session session = null, PaymentPlan paymentPlan = null, int count = 1, List<Subject> subjects = null)
        {
           var students = PopulateStudents(db, count: count, session:session,paymentPlan:paymentPlan,subjects:subjects).Build();
            Persist(students);
            return students.ToList();
        }

        internal static void AddTeachersAttendances(this MockDb db, DateTime from, DateTime to, Domain.Teacher teacher, List<Domain.Student> students = null)
        {

            while (to > from)
            {
                if(students == null)
                {
                    db.Students.First().AddAttendance(db, teacher: teacher,
                                                    date: from);    
                }
                else
                {
                    students.ForEach(x=> x.AddAttendance(db,teacher:teacher,date:from));
                }
                

                from = from.AddDays(1);
            }

        }

        public static Domain.SessionRegister AddSessionRegister( Domain.Session session = null, DateTime? date = null, SessionRegisterStatus status = SessionRegisterStatus.Pending)
        {
            var sessionRegister = new Domain.SessionRegister { Session = session, Date = date.GetValueOrDefault(), Status= status};
            Persist(sessionRegister);
            return sessionRegister;

        }

        internal static void AddAttendance(this Domain.Student student,MockDb db, Domain.Session session = null, SessionAttendanceStatus status = SessionAttendanceStatus.Attended, DateTime? date = null, Domain.Teacher teacher = null, Subject subject = null, int? paymentAmountId=null, decimal? paymentAmount = null, PaymentType? paymentType = null, Invoice invoice = null, PaymentPlan paymentPlan = null)
        {
            var numberOfSessionsPaidFor = 1;
            if (teacher == null) teacher = db.Teachers.First(x => x.Id == 1);
            if (subject == null) subject = db.Subjects.First(x => x.Id == 1);
            if (session == null) session = db.Sessions.OnDay(DayOfWeek.Monday);
            if (date == null) date = DateTime.Now.LastMonday();
            //Changes added -- Don't add pending advance Invoice
            //if (invoice == null) invoice = student.LatestPendingAdvancedInvoice();
            if (paymentAmountId == null && paymentAmount == null && paymentPlan != null)
            {
                paymentAmount = paymentPlan.Amount;
                paymentAmountId = paymentPlan.Id;
                numberOfSessionsPaidFor = paymentPlan.TotalSessions;
            }

            var attendance = CreateAttendance(student, status, teacher, subject);
            var register = AddSessionRegister(session, date);
            attendance.SessionRegister = register;
            student.SessionAttendances.Add(attendance);

            if (paymentType.HasValue)
                attendance.Payments.Add(new InvoicePayment()
                                            {
                                                PaymentAmountId = paymentAmountId.HasValue ? paymentAmountId.GetValueOrDefault() : (int?)null,
                                                PaymentAmount = paymentAmount.HasValue ? paymentAmount.GetValueOrDefault() : (decimal?)null,
                                                PaymentType = paymentType.GetValueOrDefault(),
                                                PaymentDate = DateTime.Now,
                                                Invoice = invoice,
                                                Attendance = attendance,
                                                NumberOfSessionsPaidFor = numberOfSessionsPaidFor 
                                                
                                            });




            Persist(student);
        }

        public static void AddAProcessedRegister(Domain.Student student, MockDb db)
        {
            var teacher = db.Teachers.First(x => x.Id == 1);
            var subject = db.Subjects.First(x => x.Id == 1);
            var session = db.Sessions.OnDay(DayOfWeek.Monday);
            var date = DateTime.Now.LastMonday();
            var attendance = CreateAttendance(student, SessionAttendanceStatus.Attended, teacher, subject);

            var register = AddSessionRegister(session, date, SessionRegisterStatus.Processed);
            attendance.SessionRegister = register;
            student.SessionAttendances.Add(attendance);

            Persist(student);
        }

        private static readonly
            Func< Domain.Student, SessionAttendanceStatus, Domain.Teacher, Subject,SessionAttendance>
            CreateAttendance = ( student, arg3, arg5, arg6) =>
                                   {
                                       var sa = new SessionAttendance
                                                    {
                                                        Status = arg3,
                                                        Student = student,
                                                    };

                                       sa.SubjectsAttended.Add(                                          
                                           Builder<SubjectAttendance>.CreateNew().With(sua => sua.Id = 0).
                                               And(sua => sua.Attendance = sa).
                                               And(sua => sua.Teacher = arg5).
                                               And(sua => sua.Subject = arg6).
                                               And(sua => sua.Notes = "test").
                                               Build());

                                       return sa;
                                   };
    }

    public class MockDb
    {
        public List<Domain.TuitionCentre> Centres { get; set; }
        public List<Domain.Session> Sessions { get; set; }
        public List<Domain.PaymentPlan> PaymentPlans { get; set; }
        public List<Domain.Subject> Subjects { get; set; }
        public List<Domain.Student> Students { get; set; }
        public List<Domain.Teacher> Teachers { get; set; } 
    }
}