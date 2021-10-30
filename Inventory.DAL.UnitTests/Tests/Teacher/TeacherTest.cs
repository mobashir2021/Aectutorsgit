using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.DAL.UnitTests.Helpers;
using AECMIS.Service;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Dates;
using NUnit.Framework;

namespace AECMIS.DAL.UnitTests.Tests.Teacher
{
    [TestFixture]
    public class TeacherTest:BaseTest<Domain.Teacher>
    {
        private TeacherService _teacherService;
        private TeacherRepository _teacherRepository;
        private IRepository<TeacherAttendance, int> _teacherAttendanceRepository;

        private IRepository<TeacherAttendance, int> _repository;
       
            [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _teacherAttendanceRepository = new SqLiteRepository<TeacherAttendance, int>();
            _teacherRepository  = new TeacherRepository(Session);            
            _teacherService = new TeacherService(_teacherRepository, _teacherAttendanceRepository);
                _repository =new Repository<TeacherAttendance, int>();
        }

        public override void VerifyMapping()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CanGetTeachersByStudent()
        {
            var db = BuildMockDb();
            var s1 = db.Students.First(x => x.Id == 1);
            var s2 = db.Students.First(x => x.Id == 2);

            //add 2 attendances with same teacher
            //add 2 attendances with different teacher

            s1.AddAttendance(db, teacher: db.Teachers.WithId(1),
                             subject: db.Subjects.WithName("english").AtLevel(Curriculum.Gcse));
            s1.AddAttendance(db, teacher: db.Teachers.WithId(1), session: db.Sessions.OnDay(DayOfWeek.Tuesday),
                             subject: db.Subjects.WithName("science").AtLevel(Curriculum.Gcse));

            s2.AddAttendance(db, teacher: db.Teachers.WithId(1),
                             subject: db.Subjects.WithName("english").AtLevel(Curriculum.Gcse));
            s2.AddAttendance(db, teacher: db.Teachers.WithId(2), session: db.Sessions.OnDay(DayOfWeek.Tuesday),
                             subject: db.Subjects.WithName("science").
                                 AtLevel(Curriculum.Gcse));


            var teachersByS1 = _teacherService.GetTeachersByStudent(s1.Id).ToList();
            var teachersByS2 = _teacherService.GetTeachersByStudent(s2.Id).ToList();
            Assert.AreEqual(1, teachersByS1.Count);
            Assert.AreEqual(2,teachersByS2.Count);
        }

        [Test]
        public void CanSaveTeacher()
        {
            _teacherService.SaveTeacher(new Domain.Teacher{FirstName = "Test1", LastName = "test2"});
        }

        [Test]
        public void CannotSaveDuplicateTeacher()
        {
            var db = BuildMockDb();
            var teacher =  db.Teachers.FirstOrDefault();

            Assert.Throws<Exception>(() => _teacherService.SaveTeacher(new Domain.Teacher() { FirstName = teacher.FirstName, MiddleName= teacher.MiddleName, LastName=teacher.LastName }));
        }

        [Test]
        public void CanGetTeachersByCentre()
        {
            //add session attendances by centre
            var db = BuildMockDb();
            var s1 = db.Sessions.AtCentre(1).OnDay(DayOfWeek.Monday);
            var s2 = db.Sessions.AtCentre(2).OnDay(DayOfWeek.Tuesday);

            db.Students.First().AddAttendance(db, teacher: db.Teachers.WithId(1),
                             subject: db.Subjects.WithName("english").AtLevel(Curriculum.Gcse), 
                             session:s1);
            db.Students.First().AddAttendance(db, teacher: db.Teachers.WithId(2),
                             subject: db.Subjects.WithName("science").AtLevel(Curriculum.Gcse),
                             session: s1);

            

            var teachersByS1 = _teacherService.GetTeachersByCentre(s1.Location.Id);
            var teachersByS2 = _teacherService.GetTeachersByCentre(s2.Location.Id);

            Assert.AreEqual(2,teachersByS1.Count);
            Assert.AreEqual(0,teachersByS2.Count);
        }

        [Test]
        public void CanGetTeacherSessionAttendances()
        {
            var db = BuildMockDb();
            var teacher1 = db.Teachers.WithId(1);
            var fromDate = new DateTime(2013, 2, 4, 11, 00, 00);
            var toDate = new DateTime(2013, 2, 11, 00, 00, 00);
            //add attendances for teacher
            db.AddTeachersAttendances(fromDate,toDate, teacher1);

            var attendances =  _teacherRepository.GetTeacherSessionAttendances(fromDate, toDate);

            Assert.AreEqual((int)Math.Round((toDate - fromDate).TotalDays), attendances.Count(x => x.Teacher.Id == teacher1.Id));

        }

        [Test]
        public void CanGetTeacherAttendancesWithMultipleSubjectAttendances()
        {
            var db = BuildMockDb();
            var teacher1 = db.Teachers.WithId(1);
            var fromDate = new DateTime(2013, 2, 4, 11, 00, 00);
            var toDate = new DateTime(2013, 2, 11, 00, 00, 00);
            //add attendances for teacher
            db.AddTeachersAttendances(fromDate, toDate, teacher1,
                                      new List<Domain.Student> {db.Students.WithId(1), db.Students.WithId(2)});

            var attendances = _teacherRepository.GetTeacherSessionAttendances(fromDate, toDate);

            Assert.AreEqual((int) Math.Round((toDate - fromDate).TotalDays),
                            attendances.Count(x => x.Teacher.Id == teacher1.Id));

        }

        [Test]
        public void CanGetTeacherAttendancesWithMultipleAttendancesOnSameDay()
        {
            var db = BuildMockDb();
            var teacher1 = db.Teachers.WithId(1);
            var date = new DateTime(2013, 2, 11);
            //add attendances for teacher

            db.Students.First().AddAttendance(db, teacher: teacher1,date:date, session: db.Sessions.ManyOnDay(DayOfWeek.Monday).Between(11, 13));
            db.Students.First().AddAttendance(db, teacher: teacher1,date:date, session: db.Sessions.ManyOnDay(DayOfWeek.Monday).Between(13, 15));

            var attendances = _teacherRepository.GetTeacherSessionAttendances(date, date);

            Assert.AreEqual(2,attendances.Count(x => x.Teacher.Id == teacher1.Id));
        }


        [Test]
        public void CanGetTeacherDailyWorkingStats()
        {
            var db = BuildMockDb();
            var teacher1 = db.Teachers.WithId(1);
            var fromDate = new DateTime(2013, 2, 11);

            db.Students.First().AddAttendance(db, teacher: teacher1, date: fromDate.AddDays(1),
                                              session: db.Sessions.ManyOnDay(DayOfWeek.Tuesday).Between(11, 13));
            db.Students.First().AddAttendance(db, teacher: teacher1, date: fromDate.AddDays(1),
                                              session: db.Sessions.ManyOnDay(DayOfWeek.Tuesday).Between(13, 15));

            var attendances = TeacherService.GetAttendancesByDate(fromDate.AddDays(1),
                                                                  _teacherRepository.GetTeacherSessionAttendances(
                                                                      fromDate, fromDate.AddDays(7)));

            var dailyAttendance = _teacherService.CreateNewTeacherDailyAttendance(fromDate.AddDays(1), attendances,
                                                                                  teacher1);

            Assert.AreEqual(11, dailyAttendance.StartTime.Hours);
            Assert.AreEqual(15, dailyAttendance.EndTime.Hours);
            Assert.AreEqual(2, dailyAttendance.TotalLessons);
            Assert.AreEqual(4, dailyAttendance.TotalTime);
        }

        [Test]
        public void CanGetTeacherAttendances()
        {
            var db = BuildMockDb();
            var teacher1 = db.Teachers.WithId(1);
            var fromDate = new DateTime(2013, 2, 11);

            db.Students.First().AddAttendance(db, teacher: teacher1, date: fromDate, session: db.Sessions.ManyOnDay(DayOfWeek.Monday).Between(11, 13));
            db.Students.First().AddAttendance(db, teacher: teacher1, date: fromDate.AddDays(1),
                                              session: db.Sessions.ManyOnDay(DayOfWeek.Tuesday).Between(11, 13));
            db.Students.First().AddAttendance(db, teacher: teacher1, date: fromDate.AddDays(1),
                                              session: db.Sessions.ManyOnDay(DayOfWeek.Tuesday).Between(13, 15));

            var attendances = _teacherService.GetTeacherAttendances(fromDate, fromDate.AddDays(7), teacher1);

            Assert.AreEqual(2,attendances.Count());
        }

        private static MockDb BuildMockDb()
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
            mockDb.Students = DbData.PopulateAndPersistStudents(mockDb, count: 2);

            return mockDb;
        }


        

        //private static void BuildAttendances(Domain.Student student,Domain.Teacher teacher, MockDb db)
        //{
        //    Builder<Domain.SessionAttendance>.CreateListOfSize(3).TheFirst(1).
        //        With(
        //            x =>
        //            CreateAttendance(DayOfWeek.Monday, student, SessionAttendanceStatus.Attended,
        //                           DateTime.Now.AddDays(-10), db)).
        //        TheNext(1).
        //        With(
        //            x =>
        //            CreateAttendance(DayOfWeek.Tuesday, student, SessionAttendanceStatus.Attended,
        //                           DateTime.Now.AddDays(-9), db)).
        //        TheNext(1).
        //        With(
        //            x =>
        //            CreateAttendance(DayOfWeek.Wednesday, student, SessionAttendanceStatus.Attended,
        //                           DateTime.Now.AddDays(-8), db)).
        //        All().
        //        With(x => x.Id = 0);
        //}
    }
}
