using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Criteria;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.Service;
using FluentNHibernate.Testing;
using NUnit.Framework;
using NHibernate.Linq;
using NHibernate.Criterion;
namespace AECMIS.DAL.UnitTests.Tests.Student
{
    [TestFixture]
    public class StudentTests:BaseTest<Domain.Student>
    {
        private StudentRepository _repository;
        
        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            _repository = new StudentRepository(Session);   
        }

        [Test]
        public override void VerifyMapping()
        {
            var comparer = new EqualityComparer();
            comparer.RegisterComparer((EducationInstitute x) => x.Id);
            comparer.RegisterComparer((Address x) => x.Id);
            comparer.RegisterComparer((Qualification x) => x.Id);
            comparer.RegisterComparer((PaymentPlan x) => x.Id);
            comparer.RegisterComparer((ContactPerson x) => x.Id);
            

            //add sessions and subjects
            //
            //add payment plan
            new PersistenceSpecification<Domain.Student>(Session, comparer).
                CheckProperty(x => x.Id, 1).
                CheckProperty(x => x.FirstName, "Test F Name").
                CheckProperty(x => x.LastName, "Test M Name ").
                CheckProperty(x => x.MiddleName, "test M Name").
                CheckProperty(x => x.HobbiesAndInterests, "test hobbies and interests").
                CheckProperty(x => x.Nationality, "test nationality").
                CheckProperty(x => x.IsMemberOfClubOrSociety, false).
                CheckProperty(x => x.AccessToComputer, true).
                CheckProperty(x => x.AddressVerified, true).
                CheckProperty(x => x.FirstLanguage, "Gujerati").
                CheckProperty(x => x.DiscountAmount, (decimal)5).
                CheckProperty(x => x.Age, 12).
                CheckReference(x => x.DefaultPaymentPlan, new PaymentPlan()
                                                              {
                                                                  Id = 1,
                                                                  Amount = 80,
                                                                  Curriculum = Curriculum.Gcse,
                                                                  TotalSessions = 4

                                                              })
                //                                              .
                //CheckComponentList(x => x.Contacts, new List<ContactPerson>
                //                                      {
                //                                          new ContactPerson
                //                                              {
                //                                                  ContactAddress = new Address
                //                                                                       {
                //                                                                           AddressLine1 = "Contact Address1",
                //                                                                           AddressLine2 = "Contact Address2",
                //                                                                           AddressLine3 = "Contact Address3",
                //                                                                           AddressLine4 = "Contact Address4",
                //                                                                           AddressLine5 = "Contact Address5",
                //                                                                           City = "Contact City",
                //                                                                           County = "Contact County",
                //                                                                           PostCode = "Contact PostCode"
                //                                                                       },
                //                                                                       ContactName = "Ayaz Ali",
                //                                                                       Type = RelationType.Father,
                //                                                                       ContactPhone = new Contact
                //                                                                                          {
                //                                                                                              HomeNumber = "02084721148",
                //                                                                                              Email = "ayazaliuk@hotmail.com",
                //                                                                                              MobileNumber = "07919001418",
                //                                                                                              WorkNumber = "09302223"
                //                                                                                          }
                //                                              }
                //                                      })                
                //.CheckComponentList(x => x.EducationInstitutes, new List<EducationInstitute>
                //                                         {
                //                                             new EducationInstitute
                //                                                 {
                //                                                     Address = new Address
                //                                                                   {
                //                                                                       AddressLine1 =
                //                                                                           "My School Address1",
                //                                                                       AddressLine2 =
                //                                                                           "My School Address2",
                //                                                                       AddressLine3 =
                //                                                                           "My School Address3",
                //                                                                       City = "My School City",
                //                                                                       PostCode = "My School PostCode"
                //                                                                   },
                //                                                     Name = "My School Name",
                //                                                     StudentNo = "My School StudentNo",
                //                                                     From = new DateTime(1992, 8, 1),
                //                                                     To = new DateTime(1995, 7, 31),
                //                                                     Qualifications = new List<Qualification>
                //                                                                          {
                //                                                                              new Qualification
                //                                                                                  {
                //                                                                                      Subject =
                //                                                                                          "GCSE Maths",
                //                                                                                      Result = "B",
                //                                                                                      Year = 1995                                                                                                      
                //                                                                                  },
                //                                                                                  new Qualification
                //                                                                                  {
                //                                                                                      Subject =
                //                                                                                          "GCSE English",
                //                                                                                      Result = "B",
                //                                                                                      Year = 1995                                                                                                      
                //                                                                                  }
                //                                                                          },
                //                                                     Teacher = "Mrs Stefano",
                //                                                     Type = InstituteType.Secondary

                //                                                 }
                //                                         }
                //)
                    .VerifyTheMappings();

        }

        [Test]
        public void CanAddSubjectsTest()
        {
            var centres = DbData.PopulateTuitionCentres();
            var session = DbData.PopulateSession(centres.First());
            var paymentPlan = DbData.PopulatePaymentPlan();
            var subjects = DbData.PopulateSubjects();
            
            var student = DbData.PopulateStudents(null, session,paymentPlan, 1, subjects).Build().First();

            Assert.AreEqual(1,student.Subjects.Count);
            student.Subjects.Add(new StudentSubject
                                     {
                                         Session = session,
                                         Student = student,
                                         Subject = subjects.First(x => x.Level == Curriculum.Gcse && x.Name == "Maths")
                                     });
            Session.Save(student);
            Session.Clear();
            
            Assert.AreEqual(2,student.Subjects.Count);

        }

        [Test]
        public void CanGetStudentsWithNoCredits()
        {
            var centres = DbData.PopulateTuitionCentres();
            var session = DbData.PopulateSession(centres.First());
            var paymentPlan = DbData.PopulatePaymentPlan();
            var subjects = DbData.PopulateSubjects();

            var student = DbData.PopulateAndPersistStudents(null, session, paymentPlan, 1, subjects).First();

            var results = _repository.GetStudentsWithNullCredits(new StudentSearchDto());

            Assert.IsTrue(results.Count > 0);
        }


        [Test]
        public void CanSearchStudents()
        {
            var centres = DbData.PopulateTuitionCentres();
            var session = DbData.PopulateSession(centres.First());
            var paymentPlan = DbData.PopulatePaymentPlan();
            var subjects = DbData.PopulateSubjects();

            var student = DbData.PopulateAndPersistStudents(null, session, paymentPlan, 1, subjects).First();

            var search = new StudentSearchDto { FirstName = student.FirstName, PageIndex = 0, PageSize = 10};                
            var results = _repository.SearchStudents(search);

            Assert.IsTrue(results.Students.Count > 0);
        }

        [Test]
        public void CannotChangePaymentPlanWithOpenCredits()
        {

        }

        //make sure we cannot add the same subject to the same session twice
        //only add students to sessions that teach those subjects
        //
        private List<StudentSubject> BuildFirstStudentSubjects(List<Subject> subjects, List<Domain.Session> sessions)
        {
            return new List<StudentSubject>
                       {
                           new StudentSubject
                               {
                                   Session = sessions.First(x => x.Day == DayOfWeek.Monday),
                                   Subject =
                                       subjects.First(
                                           x => x.Name == "English" && x.Level == Curriculum.Gcse)
                               },
                           new StudentSubject
                               {
                                   Session = sessions.First(x => x.Day == DayOfWeek.Tuesday),
                                   Subject =
                                       subjects.First(
                                           x => x.Name == "Science" && x.Level == Curriculum.Gcse)
                               }
                       };
        }

        private List<StudentSubject> BuildSecondStudentSubjects(List<Subject> subjects, List<Domain.Session> sessions)
        {
            return new List<StudentSubject>
                                           {
                                               new StudentSubject
                                                   {
                                                       Session = sessions.First(x => x.Day == DayOfWeek.Monday),
                                                       Subject =
                                                           subjects.First(
                                                               x => x.Name == "English" && x.Level == Curriculum.Gcse)
                                                   },
                                               new StudentSubject
                                                   {
                                                       Session = sessions.First(x => x.Day == DayOfWeek.Tuesday),
                                                       Subject =
                                                           subjects.First(
                                                               x => x.Name == "Maths" && x.Level == Curriculum.Gcse)
                                                   }
                                           };
        }
    }

    //var firstStudentSubjects = BuildFirstStudentSubjects(subjects, sessions);
    //var secondStudentSubjects = BuildSecondStudentSubjects(subjects, sessions);


    //var studentPayedFor4Sessions = DbData.GetDummyStudent("Ayaz", "Ali", firstStudentSubjects,
    //                                                    paymentPlan.First(x => x.TotalSessions == 4));
    //var studentPayedFor6Sessions = DbData.GetDummyStudent("Farees", "Ali", secondStudentSubjects,
    //                                                  paymentPlan.First(x => x.TotalSessions == 6));

    
}
