using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.UnitTests.Helpers;
using AECMIS.Service;
using AECMIS.Service.DTO;
using AECMIS.Service.Helpers;
using FluentNHibernate.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.UnitTests.Tests.TimeTable
{
    [TestFixture]
    public class TimeTableTests : BaseTest<Domain.Session>
    {
        private TimeTableService _timeTableService;
        private readonly Func<MockDb, int, Domain.Session> _session = (mock, id) => mock.Sessions.First(x => x.Id == id);
        private readonly Func<MockDb, string, Curriculum, Subject> _subject = (db, s, arg3) => db.Subjects.First(x => x.Name == s && x.Level == arg3);

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _timeTableService = new TimeTableService(Session, new SqLiteRepository<Domain.Session, int>(), new SqLiteRepository<Domain.TuitionCentre, int>(), new SqLiteRepository<Domain.Subject, int>());
        }

        [TestCase]
        public override void VerifyMapping()
        {
            var comparer = new EqualityComparer();
            comparer.RegisterComparer((Subject x) => x.Id);
            new PersistenceSpecification<Domain.Session>(Session).
                CheckProperty(x => x.Id, 1).
                CheckProperty(x => x.Day, DayOfWeek.Thursday).
                CheckProperty(x => x.From, new TimeSpan(11, 00, 00)).
                CheckProperty(x => x.To, new TimeSpan(13, 00, 00)).
                CheckComponentList(x => x.SubjectsTaughtAtSession, new List<Subject>()
                                                                       {
                                                                           new Subject
                                                                               {
                                                                                   Name = "Maths",
                                                                                   Level = Curriculum.Gcse
                                                                               },
                                                                           new Subject
                                                                               {
                                                                                   Name = "English",
                                                                                   Level = Curriculum.Gcse
                                                                               }
                                                                       }, comparer).
                VerifyTheMappings();
        }


        [Test]
        public void CanSearchSubjects()
        {
            var db = BuildMockDb(true, true);
            var subjectName = "Maths";
            var subjectLevel = Curriculum.Gcse;
            
            var subjectLevelSearchResult = _timeTableService.FindSubjects(new SearchSubjectDto() { Level = subjectLevel });
            var subjectNameSearchResult = _timeTableService.FindSubjects(new SearchSubjectDto() {  Name = subjectName });

            Assert.IsTrue(subjectLevelSearchResult.Count == db.Subjects.FindAll(x => x.Level == subjectLevel).Count());
            Assert.IsTrue(subjectNameSearchResult.Count == db.Subjects.FindAll(x => x.Name == subjectName).Count());
        }

        [Test]
        public void CanSaveSubject()
        {
            var db = BuildMockDb(true, true);
            var subjectName = "Politics";
            var subjectLevel = Curriculum.ALevel;

            _timeTableService.CreateSubject(new SubjectViewModel() { Name = subjectName, Level = subjectLevel });

            Assert.IsTrue(_timeTableService.FindSubjects(new SearchSubjectDto() { Level = subjectLevel, Name = subjectName }).Count() == 1);
        }

        [Test]
        public void CanGetSessions()
        {
            var db = BuildMockDb(true, true);
            var sessions = _timeTableService.GetAllSessions();
            Assert.IsTrue(sessions.Count == db.Sessions.Count);
        }

        [Test]
        public void CanSearchSessions()
        {
            var db = BuildMockDb(true, true);
            var session = _session(db, 3);
            var sessions = _timeTableService.FindSessions(new SearchSessionDto() { centreId = session.Location.Id, dayOfWeek = session.Day });

            Assert.IsTrue(sessions.Count == db.Sessions.FindAll(x => x.Day == session.Day && x.Location.Id == session.Location.Id).Count());
        }

        [Test]
        public void CannotCreateDuplicateSession()
        {
            var db = BuildMockDb(true, true);
            var session = _session(db, 3);

            var ex = Assert.Throws<Exception>(() => _timeTableService.SaveSession(new SessionViewModel() { From = session.From, To = session.To, Day = session.Day, Location = session.Location.Id }));
            Assert.That(ex.Message, Is.EqualTo(Constants.DUPLICATE_SESSION_MESSAGE));

        }

        [Test]
        public void CannotCreateSessionWithInvalidCentre()
        {
            var db = BuildMockDb(true, true);
            var session = _session(db, 3);

            var ex = Assert.Throws<Exception>(() => _timeTableService.SaveSession(new SessionViewModel() { From = session.From, To = session.To, Day = session.Day, Location = 10 }));
            Assert.That(ex.Message, Is.EqualTo(Constants.INVALID_CENTRE_MESSAGE));
        }

        [Test]
        public void CanCreateSession()
        {
            var db = BuildMockDb(true, true);
            var centre = db.Centres.FirstOrDefault();

            var session = new SessionViewModel() { Day = DayOfWeek.Saturday, Location = centre.Id, From = new TimeSpan(15, 0, 0), To = new TimeSpan(17, 00, 00) };

            Assert.IsFalse(_timeTableService.FindSessions(new SearchSessionDto() { dayOfWeek = session.Day, centreId = session.Location }).Count() == 1);

            _timeTableService.SaveSession(session);

            Assert.IsTrue(_timeTableService.FindSessions(new SearchSessionDto() { dayOfWeek = session.Day, centreId = session.Location }).Count() == 1);

        }

        [Test]
        public void CanCreateSessionWithNewAndExistingSubjects()
        {
            var db = BuildMockDb(true, true);
            var centre = db.Centres.FirstOrDefault();
            var subject = _subject(db, "Maths", Curriculum.Gcse);
            var newSubjectName = "A test1";
            var existingSubjectViewModel = new SubjectViewModel() { SubjectId = subject.Id, Name = subject.Name, Level = subject.Level };
            var newSubjectViewModel = new SubjectViewModel() { Name = newSubjectName, Level = Curriculum.ElevenPlus };

            var session = new SessionViewModel() { Day = DayOfWeek.Saturday, Location = centre.Id, From = new TimeSpan(15, 0, 0), To = new TimeSpan(17, 00, 00), Subjects = new List<SubjectViewModel>() { { existingSubjectViewModel }, { newSubjectViewModel } } };
            Assert.IsFalse(_timeTableService.FindSessions(new SearchSessionDto() { dayOfWeek = session.Day, centreId = session.Location }).Count() == 1);

            _timeTableService.SaveSession(session);

            var newSession = _timeTableService.FindSessions(new SearchSessionDto() { dayOfWeek = session.Day, centreId = session.Location }).FirstOrDefault();

            Assert.IsTrue(newSession != null && newSession.SubjectsTaughtAtSession != null && newSession.SubjectsTaughtAtSession[0].Id == subject.Id && newSession.SubjectsTaughtAtSession.Any(p => p.Name == newSubjectName));

        }

        [Test]
        public void CannotCreateSessionWithDuplicateSubject()
        {
            var db = BuildMockDb(true, true);
            var centre = db.Centres.FirstOrDefault();
            var subject = _subject(db, "Maths", Curriculum.Gcse);
            var newSubjectViewModel = new SubjectViewModel() { Name = subject.Name, Level = subject.Level };

            var session = new SessionViewModel() { Day = DayOfWeek.Saturday, Location = centre.Id, From = new TimeSpan(15, 0, 0), To = new TimeSpan(17, 00, 00), Subjects = new List<SubjectViewModel>() { { newSubjectViewModel } } };
            Assert.Throws<Exception>(() => _timeTableService.SaveSession(session));
        }

        [Test]
        public void CanUpdateExistingSessionWithNewSubjects()
        {
            var db = BuildMockDb(true, true);
            var session = _session(db, 3);
            var newSubjectName = "A test1";

            var newSubjectViewModel = new SubjectViewModel() { Name = newSubjectName, Level = Curriculum.ElevenPlus };
            var sessionViewModel = new SessionViewModel() { SessionId = session.Id, Day = session.Day, Location = session.Location.Id, From = session.From, To = session.To, Subjects = new List<SubjectViewModel>() { { newSubjectViewModel } } };

            _timeTableService.SaveSession(sessionViewModel);
            var updatedSession = _timeTableService.GetSession(session.Id);

            Assert.IsTrue(updatedSession != null && updatedSession.SubjectsTaughtAtSession != null && updatedSession.SubjectsTaughtAtSession[0].Name == newSubjectName && updatedSession.SubjectsTaughtAtSession.Count() == 1);
        }


        private static MockDb BuildMockDb(bool? populateStudents = null, bool? populateInvoice = null)
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
                mockDb.Students = DbData.PopulateAndPersistStudents(mockDb, count: 2);
            return mockDb;
        }
    }
}
