using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.DAL.UnitTests.Helpers;
using AECMIS.Service;
using AECMIS.Service.Helpers;
using FizzWare.NBuilder;
using FluentNHibernate.Testing;
using AECMIS.Service.DTO;

using NUnit.Framework;


namespace AECMIS.DAL.UnitTests.Tests.SessionRegister
{
    [TestFixture]
    public class SessionRegisterTests : BaseTest<Domain.SessionRegister>
    {
        private SessionRegisterService _sessionRegisterService;

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            var attendanceRepository = new SessionAttendanceRepository(Session);
            _sessionRegisterService = new SessionRegisterService(Session,new SqLiteRepository<Domain.Session, int>(),
                                                 new SqLiteRepository<Domain.Student, int>(),
                                                attendanceRepository, 
                                                 new SqLiteRepository<PaymentPlan, int>(),
                                                 new SqLiteRepository<Domain.SessionRegister, int>(),
                                                 new SqLiteRepository<Domain.Teacher, int>(),
                                                 new SqLiteRepository<Domain.TuitionCentre,int>());

        }

      

        [TestCase]
        public override void VerifyMapping()
        {
            //Needs to be rewritten for SessionRegister Type
            //var comparer = new EqualityComparer();
            //comparer.RegisterComparer((Subject x) => x.Id);
            //new PersistenceSpecification<Domain.Session>(Session).
            //    CheckProperty(x => x.Id, 1).
            //    CheckProperty(x => x.Day, DayOfWeek.Thursday).
            //    CheckProperty(x => x.From, new TimeSpan(11, 00, 00)).
            //    CheckProperty(x => x.To, new TimeSpan(13, 00, 00)).
            //    CheckComponentList(x => x.SubjectsTaughtAtSession, new List<Subject>()
            //                                                           {
            //                                                               new Subject
            //                                                                   {
            //                                                                       Name = "Maths",
            //                                                                       Level = Curriculum.Gcse
            //                                                                   },
            //                                                               new Subject
            //                                                                   {
            //                                                                       Name = "English",
            //                                                                       Level = Curriculum.Gcse
            //                                                                   }
            //                                                           }, comparer).
            //    VerifyTheMappings();
        }


        private readonly Func<MockDb, int, Domain.Session> _session = (mock, id) => mock.Sessions.First(x => x.Id == id);
        private readonly Func<MockDb, string,Curriculum, Subject> _subject = (db, s, arg3) => db.Subjects.First(x => x.Name == s && x.Level == arg3);
        private readonly Func<MockDb, int, PaymentPlan> _paymentPlan = (db, i) => db.PaymentPlans.First(x => x.TotalSessions == i);
        private readonly Func<MockDb, int, Domain.Teacher> _teacher = (db, i) => db.Teachers.First(x => x.Id == i);
            
        [Test]
        public void CanGetSessionRegister()
        {
            const int studentsInSession = 10;
            var mockDb = BuildMockDb();
            var session = _session(mockDb, 1);

            PopulateSessionWithStudents(studentsInSession, session, _subject(mockDb, "English", Curriculum.Gcse),_paymentPlan(mockDb, 4));

            var studentSubjects = _sessionRegisterService.GetRegister(session.Id, DateTime.Today).SessionAttendees;

            Assert.AreEqual(studentsInSession, studentSubjects.Count);
        }

        //[Test]
        //public void CanGetSessionRegisterWithUnpaidInvoices()
        //{
        //    const int studentsInSession = 10;
        //    const int studentsThatNeedToPay = 5;
        //    var mockDb = BuildMockDb();
        //    var session = _session(mockDb, 1);

        //    PopulateSessionWithStudentsAndUnpaidInvoices(studentsInSession, studentsThatNeedToPay, session,
        //                                                 _subject(mockDb, "English", Curriculum.Gcse),
        //                                                 _paymentPlan(mockDb, 4));

        //    var studentSubjects = _sessionService.GetRegister(session.Id, DateTime.Today).SessionAttendees;

        //    Assert.AreEqual(studentsThatNeedToPay, studentSubjects.Count(x => x.SessionAttendanceViewModel.PaymentRequired));
        //}
        [Test]
        public void CanGetSessionRegisterWithUnpaidInvoices()
        {
            const int studentsInSession = 10;
            const int studentsThatNeedToPay = 5;
            var mockDb = BuildMockDb();
            var session = _session(mockDb, 1);

            PopulateSessionWithStudentsAndUnpaidInvoices(studentsInSession, studentsThatNeedToPay, session,
                _subject(mockDb, "English", Curriculum.Gcse),
                _paymentPlan(mockDb, 4));

            var studentSubjects = _sessionRegisterService.GetRegister(session.Id, DateTime.Today).SessionAttendees;

            Assert.AreEqual(studentsThatNeedToPay, studentSubjects.Count(x => x.SessionAttendanceViewModel.FutureInvoicePayment.PaymentPlans.Count>0));
        }
        //TODO
        [Test]
        public void CanGetSessionRegisterWithPayments()
        {

        }

        //TODO:HasMany does not work in SQLite
        [Ignore("")]
        [Test]
        public void CanGetSessionRegisterWithExistingAttendances()
        {
            const int studentsInSession = 10;
            const int attendancesInSession = 5;
            var mockDb = BuildMockDb();
            var session = _session(mockDb, 1);

            PopulateSessionWithAttendances(studentsInSession, attendancesInSession, session,
                                           _subject(mockDb, "English", Curriculum.Gcse), _paymentPlan(mockDb, 4),
                                           _teacher(mockDb, 1));

            var studentSubjects = _sessionRegisterService.GetRegister(session.Id, DateTime.Today).SessionAttendees;
            Assert.AreEqual(attendancesInSession, studentSubjects.Count(x => x.SessionAttendanceViewModel !=null));
        }

        //[Test]
        //public void CanProcessAPayment()
        //{
        //    var db = BuildMockDb(true,true);
        //    var student = db.Students.First();

        //    var invoice = student.Invoices.First();
        //    student.AddAttendance(db, paymentAmountId: student.DefaultPaymentPlan.Id,paymentAmount:invoice.TotalAfterDiscount, paymentType:PaymentType.Cash,  invoice:invoice);
        //    _sessionService.ProcessPayment(student.SessionAttendances.ToList(),student);


        //    Assert.IsNotNull(invoice.PaymentReciept);
        //    Assert.IsTrue(invoice.Status == InvoiceStatus.Paid);
        //    Assert.AreEqual(student.DiscountedPayment(),invoice.PaymentReciept.InvoicePayment.PaymentAmount);
        //}
        [Test]
        public void CanProcessAPayment()
        {
            var db = BuildMockDb(true, true);
            var student = db.Students.First();

            student.AddAttendance(db, paymentAmountId: student.DefaultPaymentPlan.Id, paymentAmount: student.DiscountedPayment(), paymentType: PaymentType.Cash);
            _sessionRegisterService.ProcessPayment(student.SessionAttendances.ToList(), student);
            var invoice = student.Invoices.OrderByDescending(x => x.Id).First();

            Assert.IsNotNull(invoice.PaymentReciept);
            Assert.IsTrue(invoice.Status == InvoiceStatus.Paid);
            Assert.AreEqual(student.DiscountedPayment(), invoice.PaymentReciept.InvoicePayment.PaymentAmount);
        }
        //Changes added -- Commented existing and added new test
        //[Test]
        //public void CanConvertAdvancedInvoiceToDailyIfNoPayment()
        //{
        //    var db = BuildMockDb(true, true);
        //    var student = db.Students.First();
        //    var invoice = student.Invoices.First();

        //    student.AddAttendance(db, paymentType: PaymentType.None, invoice: invoice);

        //    _sessionService.ProcessPayment(student.SessionAttendances.ToList(), student);

        //    Assert.Null(invoice.PaymentReciept);
        //    Assert.IsTrue(invoice.Status == InvoiceStatus.Pending);
        //    Assert.IsTrue(invoice.PaymentType == InvoicePaymentType.Daily);


        //}
        [Test]
        public void CanCreateOutstandingDailyInvoiceIfNoPayment()
        {
            var db = BuildMockDb(true, true);
            var student = db.Students.First();

            student.AddAttendance(db,paymentType:PaymentType.None);

            _sessionRegisterService.ProcessPayment(student.SessionAttendances.ToList(), student);
            Invoice invoice = student.SessionAttendances.Select(x=>x.Invoice).FirstOrDefault();

            Assert.Null(invoice?.PaymentReciept);
            Assert.IsTrue(invoice?.Status == InvoiceStatus.Pending);
            Assert.IsTrue(invoice?.PaymentType == InvoicePaymentType.Daily);
            

        }

        //[Test]
        //public void CanHandleDeferredPayment()
        //{
        //    var db = BuildMockDb(true, true);
        //    var student = db.Students.First();
        //    var invoice = student.Invoices.First();

        //    student.AddAttendance(db, paymentType: PaymentType.None, invoice: invoice);

        //    _sessionService.ProcessStudentRegisters(new List<int>() { student.Id });
        //    Assert.Null(invoice.PaymentReciept);
        //    Assert.IsTrue(invoice.Status == InvoiceStatus.Pending);
        //    Assert.IsTrue(invoice.PaymentType == InvoicePaymentType.Daily);
        //    Assert.IsTrue(
        //        student.Invoices.Count(
        //            x => x.PaymentType == InvoicePaymentType.Advanced && x.Status == InvoiceStatus.Pending) == 1);
        //}
        [Test]
        public void CanHandleDeferredPayment()
        {
            var db = BuildMockDb(true, true);
            var student = db.Students.First();

            student.AddAttendance(db, paymentType: PaymentType.None);

            _sessionRegisterService.ProcessStudentRegisters(new List<int>() { student.Id });
            var invoice = student.Invoices.OrderByDescending(x => x.Id).First();

            Assert.Null(invoice.PaymentReciept);
            Assert.IsTrue(invoice.Status == InvoiceStatus.Pending);
            Assert.IsTrue(invoice.PaymentType == InvoicePaymentType.Daily);
            Assert.IsTrue(
                student.Invoices.Count(
                    x => x.PaymentType == InvoicePaymentType.Advanced && x.Status == InvoiceStatus.Pending) == 1);
        }



        //TODO
        [Test]
        public void StudentOnSpecialPaymentPlanTest()
        {

        }

        //TODO
        [Test]
        public void CannotGetInactiveStudentsOnRegister()
        {

        }


        //add in student with a future paid invoice
        //add in a student with a outstanding invoice
        //add in a student with a paid invoice and an outstanding invoice
        //add in a student on a daily payment plan and a single daily invoice
        [Test]
        public void CanProcessPaymentForFutureInvoice()
        {
            // add student with a paid future invoice > 1 credit 
            // add register with attended status
            // process 

            //should associate paid invoice to attendance && reduce number of sessions remaining
        }
        //Added new test
        [Test]
        public void CanProcessPaymentForStudentOnFuturePlan()
        {
            var db = BuildMockDb(false, false);
            var totalSessions = 6;
            var paymentPlan = db.PaymentPlans.First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions == totalSessions);
            //add student on a daily plan and 1 future invoice
            var student = DbData.PopulateAndPersistStudents(db, paymentPlan: paymentPlan).First();

            //add attendance and payment
            student.AddAttendance(db, paymentPlan: paymentPlan, paymentType: PaymentType.Cash);

            //process payments for students on this register
            _sessionRegisterService.ProcessStudentRegisters(new List<int>() { student.Id });

            Assert.IsTrue(student.Invoices.Count(x => x.Status == InvoiceStatus.Paid && x.PaymentReciept != null) == 1);
            Assert.IsTrue(student.AttendanceCredits().Count == totalSessions);
            Assert.IsTrue(student.AttendanceCredits().Count(x => student.SessionAttendances.Any(y => y == x.Attendance))==1);
            Assert.IsTrue(student.AttendanceCredits().Count(x=>x.Attendance==null) == totalSessions-1);
        }

        [Test]
        public void CanProcessPaymentForStudentOnBasicPlan()
        {
            var db = BuildMockDb(false, false);
            var paymentPlan = db.PaymentPlans.First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions == 1);
            //add student on a daily plan and 1 future invoice
            var student = DbData.PopulateAndPersistStudents(db, paymentPlan: paymentPlan).First();

            //add attendance and payment
            student.AddAttendance(db, paymentPlan: paymentPlan, paymentType: PaymentType.Cash);

            //process payments for students on this register
            _sessionRegisterService.ProcessStudentRegisters(new List<int>() {student.Id});

            Assert.IsTrue(student.Invoices.Count(x => x.Status == InvoiceStatus.Paid && x.PaymentReciept != null) == 1);
        }
        //[Test]
        //public void CanProcessPaymentForStudentOnBasicPlan()
        //{
        //    var db = BuildMockDb(false, false);
        //    var paymentPlan = db.PaymentPlans.
        //        First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions == 1);
        //    //add student on a daily plan and 1 future invoice
        //    var student = DbData.PopulateAndPersistStudents(db, paymentPlan:paymentPlan).First();

        //    //add attendance and payment
        //    student.AddAttendance(db,paymentPlan:paymentPlan, paymentType:PaymentType.Cash);

        //    //process payments for students on this register
        //    _sessionService.ProcessStudentRegisters(new List<int>() { student.Id });

        //    Assert.IsTrue(student.Invoices.Count(x => x.Status == InvoiceStatus.Paid && x.PaymentReciept != null) == 1);
        //    Assert.IsTrue(student.Invoices.Count(x => x.Status == InvoiceStatus.Pending && x.PaymentReciept ==null) ==1);
        //}

        //[Test]
        //public void CanProcessPaymentForDailyInvoice()
        //{
        //    var db = BuildMockDb(false, false);
        //    var paymentPlan = db.PaymentPlans.
        //        First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions == 1);

        //    var student = DbData.PopulateStudents(db, paymentPlan: paymentPlan).Build().First();
        //    student.Invoices.Clear();
        //    var invoice = BuildInvoiceAndUpdateStudent(student, db, paymentType: InvoicePaymentType.Daily, paymentPlan: paymentPlan);

        //    //add attendance and payment
        //    student.AddAttendance(db, paymentPlan: paymentPlan, paymentType: PaymentType.Cash, invoice:invoice);

        //    //process payments for students on this register
        //    _sessionService.ProcessStudentRegisters(new List<int>() { student.Id });

        //    Assert.IsTrue(student.Invoices.Count(x => x.PaymentType == InvoicePaymentType.Daily && x.Status == InvoiceStatus.Paid &&  x.PaymentReciept != null) == 1);
        //    Assert.IsTrue(student.Invoices.Count(x => x.Status == InvoiceStatus.Pending && x.PaymentReciept == null && x.PaymentType == InvoicePaymentType.Advanced) == 1);
        //}

        [Test]
        public void CanProcessPaymentForDailyInvoice()
        {
            var db = BuildMockDb(false, false);
            var paymentPlan = db.PaymentPlans.First(x => x.Curriculum == Curriculum.Gcse && x.TotalSessions == 1);

            var student = DbData.PopulateStudents(db, paymentPlan: paymentPlan).Build().First();
            student.Invoices.Clear();
           
            //add attendance and payment
            student.AddAttendance(db, paymentPlan: paymentPlan, paymentType: PaymentType.Cash);

            //process payments for students on this register
            _sessionRegisterService.ProcessStudentRegisters(new List<int>() {student.Id});

            Assert.IsTrue(student.Invoices.Count(x =>
                              x.PaymentType == InvoicePaymentType.Daily && x.Status == InvoiceStatus.Paid &&
                              x.PaymentReciept != null) == 1);
        }

        [Test]
        public void ProcessPaymentforMultipleUnprocessedAttendances()
        {
        }

        [Test]
        public void CannotProcessRegisterWithSameStudentsOnAnotherUnprocessedRegister()
        {

        }


        [Test]
        public void CanSaveRegisterRegression()
        {
            var db = BuildMockDb(true, true);
            var viewModel = BuildMockViewModel(db);

            Assert.IsTrue(_sessionRegisterService.SaveSessionAttendances(viewModel, 100) > 0);
        }

        [Test]
        public void CanProcessRegisterRegression()
        {
            var db = BuildMockDb(true, true);
            var viewModel = BuildMockViewModel(db);
            _sessionRegisterService.ProcessRegister(viewModel, 100);
        }

        private static Invoice BuildInvoiceAndUpdateStudent(Domain.Student student, MockDb db, DateTime? generationDate = null, InvoicePaymentType paymentType = InvoicePaymentType.Advanced, InvoiceStatus status = InvoiceStatus.Pending, PaymentPlan paymentPlan = null, decimal discountAmount = 0)
        {
            var invoice = BuildInvoice(db, generationDate, paymentType, status, paymentPlan, discountAmount).Build();
            invoice.Student = student;
            student.Invoices.Add(invoice);
            DbData.Persist(student);

            return invoice;
        }


        private static ISingleObjectBuilder<Invoice> BuildInvoice(MockDb db, DateTime? generationDate = null, InvoicePaymentType paymentType  = InvoicePaymentType.Advanced, InvoiceStatus status = InvoiceStatus.Pending, PaymentPlan paymentPlan = null, decimal discountAmount = 0 )
        {
            paymentPlan = paymentPlan ?? db.PaymentPlans.First(x => x.Curriculum == Curriculum.Gcse);
            generationDate = generationDate ?? DateTime.Today;

            return Builder<Invoice>.CreateNew()
                .With(x => x.Id = 0)
                .With(x => x.Status = status)
                .With(x => x.DateOfGeneration = generationDate.Value)
                .With(x => x.PaymentType = paymentType)
                .With(x => x.TotalAmount = paymentPlan.Amount)
                .With(x => x.NumberOfSessionsPayingFor = paymentPlan.TotalSessions)
                .With(x => x.TotalAfterDiscount = paymentPlan.Amount - discountAmount);
        }


        private static PaymentViewModel FuturePaymentViewModel(Domain.Student student,MockDb db, string paymentDate,PaymentType paymentType = PaymentType.Cash ,bool isFutureInvoice = true)
        {
            return new PaymentViewModel()
                       {
                        PaymentType   = paymentType,
                        PaymentAmount = student.PaymentPlanWithSessions(db, 1).Id,
                        InvoiceId = student.LatestPendingAdvancedInvoice().Id,
                        IsFutureInvoice = isFutureInvoice,
                        PaymentDate = paymentDate
                       };
        }

        private static ISingleObjectBuilder<SessionSubjectAttendanceModel> BuildSessionSubjectModel(Domain.Student student, MockDb db, bool paymentRequired = true, int? subjectId = null, int? teacherId = null, SessionAttendanceStatus sessionStatus = SessionAttendanceStatus.Attended)
        {
            return Builder<SessionSubjectAttendanceModel>
                .CreateNew()
                .With(x => x.StudentId = student.Id)
                .With(x=> x.PaymentRequired = paymentRequired)
                .With(x => x.TeacherId = db.Teachers.First().Id)
                .With(x => x.Status = sessionStatus)
                .With(x => x.SubjectId = subjectId??student.Subjects.First().Subject.Id);
        }

        private static SessionRegisterViewModel BuildMockViewModel(MockDb db)
        {
            var student = db.Students.WithAPendingAdvancedInvoice();
            string date = DateTime.Now.LastMonday().ToString("dd/MM/yyyy");
            
           return Builder<SessionRegisterViewModel>.CreateNew()
               .With(x=> x.SessionRegisterId = 0)
                .With(x => x.Center = db.Centres.First().Id)
                .With(x => x.SessionId = db.Sessions.First().Id)
                .With(x => x.Date = date)
                .With(x => x.SessionAttendees = Builder<StudentSubjectViewModel>
                    .CreateListOfSize(1)
                    .TheFirst(1).With(p=> p.StudentId= student.Id)
                    .With(p=> p.SessionAttendanceViewModel = BuildSessionSubjectModel(student,db)
                    .With(i => i.FutureInvoicePayment =
                        FuturePaymentViewModel(student, db, date)).Build())              
                    .Build().ToList()).Build();
        }

        private static MockDb BuildMockDb(bool? populateStudents=null, bool? populateInvoice=null)
        {
            var mockDb = new MockDb
            {
                Centres = DbData.PopulateTuitionCentres()
            };

            mockDb.Sessions = DbData.PopulateSessions(mockDb.Centres.First());
            mockDb.Sessions.AddRange(DbData.PopulateSessions(mockDb.Centres.ElementAt(1)));
            mockDb.Sessions.AddRange(DbData.PopulateSessions(mockDb.Centres.ElementAt(2)));
            mockDb.PaymentPlans = DbData.PopulatePaymentPlans();
            mockDb.Subjects = DbData.PopulateSubjects();
            mockDb.Teachers = DbData.PopulateTeachers(10);
            DbData.PopulateSubjectsTaughtAtSessions(mockDb.Subjects, mockDb.Sessions);

            if (populateStudents.HasValue && populateStudents.Value)
                mockDb.Students = DbData.PopulateAndPersistStudents(mockDb,count: 2);
            return mockDb;
        }


        public void PopulateSessionWithAttendances(int noOfStudentsToPopulate, int noOfAttendances, Domain.Session session, Subject subject, PaymentPlan paymentPlan, Domain.Teacher teacher)
        {
            var studentsWithAttendance = 0;

            var sessionRegister = DbData.AddSessionRegister(session, DateTime.Today);
            for (int i = 0; i < noOfStudentsToPopulate; i++)
            {
                if (studentsWithAttendance < noOfAttendances)
                {
                    DbData.GetDummyStudentWithAttendance(BuildSessionSubjects(subject, session), subject.Id,session.Id, paymentPlan,
                                                         teacher, 120, 10, 5,sessionRegister);
                    studentsWithAttendance++;
                }

                var name = "TestStudent" + i;
                DbData.GetDummyStudent(name, name, BuildSessionSubjects(subject, session), paymentPlan);
            }
        }

        private void PopulateSessionWithStudentsAndUnpaidInvoices(int noOfStudentsToPopulate, int noOfUnpaidInvoices, Domain.Session session, Domain.Subject subject, Domain.PaymentPlan paymentPlan)
        {
            var studentsWithInvoices = 0;
            for (int i = 0; i < noOfStudentsToPopulate; i++)
            {                
                if(studentsWithInvoices < noOfUnpaidInvoices)
                {
                    DbData.GetDummyStudentWithInvoice(BuildSessionSubjects(subject, session), paymentPlan, 120, 10, 5);
                    studentsWithInvoices++;
                    continue;
                }

                var name = "TestStudent" + i;
                DbData.GetDummyStudent(name, name, BuildSessionSubjects(subject, session), paymentPlan);
            }
        }

        private void PopulateSessionWithStudents(int noOfStudentsToPopulate, Domain.Session session, Domain.Subject subject, Domain.PaymentPlan paymentPlan)
        {
            for (int i = 0; i < noOfStudentsToPopulate; i++)
            {
                var name = "TestStudent" + i;
                DbData.GetDummyStudent(name, name, BuildSessionSubjects(subject, session), paymentPlan);
            }
        }

        private List<StudentSubject> BuildSessionSubjects(Subject subject, Domain.Session session)
        {
            return new List<StudentSubject>
                       {
                           new StudentSubject
                               {
                                   Session = session,
                                   Subject =subject
                                       //subjects.First(
                                       //    x => x.Name == "English" && x.Level == Curriculum.Gcse)
                               },
                       };
        }


    }
}