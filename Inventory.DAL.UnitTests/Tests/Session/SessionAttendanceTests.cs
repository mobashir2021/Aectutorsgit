using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.Service;
using FluentNHibernate.Testing;
using NUnit.Framework;
using System.Linq;
namespace AECMIS.DAL.UnitTests.Tests.Session
{
    [TestFixture]
    public class SessionAttendanceTests : BaseTest<SessionAttendance>
    {
        private SessionAttendanceRepository _sessionAttendanceRepository;
 
        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _sessionAttendanceRepository = new SessionAttendanceRepository(Session);
        }

        [Test]
        public override void VerifyMapping()
        {
            var comparer = new EqualityComparer();
            comparer.RegisterComparer((SessionAttendance x) => x.Id);
            comparer.RegisterComparer((SubjectAttendance x) => x.Id);
            comparer.RegisterComparer((Domain.Session x) => x.Id);
           comparer.RegisterComparer((Domain.Student x) => x.Id);
          comparer.RegisterComparer((Domain.SessionRegister x)=> x.Id );


            var centre = DbData.PopulateTuitionCentres().First();
            var session = DbData.PopulateSession(centre);
            var subjects = DbData.PopulateSubjects();
            var teacher = DbData.PopulateTeacher();
            var paymentplan = DbData.PopulatePaymentPlan();
            var student = DbData.PopulateStudents(null,session,paymentplan,1, subjects).Build().First();
            var register = new Domain.SessionRegister() {Session = session};

            new PersistenceSpecification<SessionAttendance>(Session, comparer).
                CheckReference(x=> x.SessionRegister,register).
               CheckReference(x => x.SessionRegister.Session, session).
                CheckReference(x => x.Student, student).
                CheckProperty(x => x.Status, SessionAttendanceStatus.Attended).
                CheckProperty(x => x.SessionRegister.Date, new DateTime(2012, 10, 10)).
                //CheckComponentList(x => x.SubjectsAttended, new List<SubjectAttendance>
                //                                                {
                //                                                    new SubjectAttendance
                //                                                        {
                //                                                            Subject =
                //                                                                subjects.First(
                //                                                                    x =>
                //                                                                    x.Level == Curriculum.Gcse &&
                //                                                                    x.Name == "English"),
                //                                                            Teacher = teacher,
                //                                                            Notes = "not good"
                //                                                        }
                //                                                }).
                VerifyTheMappings();
        }

        [Test]
        public void SearchRegisters()
        {
            _sessionAttendanceRepository.SearchForSessionRegisters(new SearchRegisterDto(){CentreId = 1,SessionId  = 9});
        }

        [Test]
        public void SearchAttendances()
        {
            var mockDb = BuildMockDb(true, true, true);
            var student = mockDb.Students.First();           

           var result =  _sessionAttendanceRepository.GetAttendances(new SearchStudentAttendanceCriteria() { Dob = DateTime.Today.Date, PageSize =10, PageIndex =0 });
            Assert.IsTrue(result.Attendances.Count > 0);            
        }


        [Test]
        public void Does_Get_Previous_Notes_From_Only_Processed_Registers()
        {
            var mockDb = BuildMockDb(true, true, true);
            var studentOnProcessedRegister = mockDb.Students.First();
            var studentOnPendingRegister = mockDb.Students.First(x => x.Id != studentOnProcessedRegister.Id);
            DbData.AddAProcessedRegister(studentOnProcessedRegister, mockDb);

            var notesFromStudentOnProcessedRegister = _sessionAttendanceRepository.GetPreviousNotes(new List<int> { studentOnProcessedRegister.Id }, DateTime.Now);
            var notesFromStudentOnPendingRegister = _sessionAttendanceRepository.GetPreviousNotes(new List<int> { studentOnPendingRegister.Id }, DateTime.Now);
            //var j = notes.OrderByDescending(x => x.Date).Select(p => string.Format("{0}-{1}/r/n", p.Date.ToString("yyyy-MM-dd"), p.Note)).Aggregate((i, o) => i + o);

            Assert.IsTrue(notesFromStudentOnProcessedRegister.Count() == 1);
            Assert.IsFalse(notesFromStudentOnPendingRegister.Count() == 1);
        }

        [Test]
        public void CanSaveAttendanceWithPayment()
        {
            var mockDb = BuildMockDb(true,true,true);
            var student = mockDb.Students.First();            
            var invoice = student.Invoices.First();
            var attendance = _sessionAttendanceRepository.Get(student.SessionAttendances.First().Id);
           

            var payment = new InvoicePayment
                              {
                                  PaymentAmount = invoice.TotalAfterDiscount,
                                  Attendance = attendance,
                                  PaymentType = PaymentType.Cash,
                                  NumberOfSessionsPaidFor = invoice.NumberOfSessionsPayingFor,
                                  Invoice = invoice,
                                  PaymentDate = DateTime.Now
                              };
            attendance.Payments.Add(payment);
            _sessionAttendanceRepository.Save(attendance);
            
            Assert.IsTrue(payment.Id > 0);
        }


        private static MockDb BuildMockDb(bool? populateStudents = null, bool? populateInvoice = null, bool? addAttendance = null)
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

            if(addAttendance.HasValue && addAttendance.Value)
                mockDb.Students.ForEach(x=> x.AddAttendance(mockDb));

            return mockDb;
        }

        


    }

    //when status is present use session credit and assign invoice payment
    //when status is unauthorizedabsence use session credit and assign invoice payment
    //when status is authorisedansence donot use session credit



}
