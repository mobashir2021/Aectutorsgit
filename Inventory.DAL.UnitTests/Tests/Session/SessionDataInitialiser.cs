using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.UnitTests.Helpers;
using Machine.Specifications;

namespace AECMIS.DAL.UnitTests.Tests.Session
{
    public abstract class SessionDataInitialiser : TestBase<Domain.Session>
    {
        protected static Domain.TuitionCentre DummyTuitionCentre;
        protected static Domain.Session ExistingSession;
         
        public Establish context = () =>
                                       {
                                           var tuitionRepository = new SqLiteRepository<Domain.TuitionCentre, int>();
                                           DummyTuitionCentre = DbData.GetDummyTuitionCentre();
                                           tuitionRepository.Save(DummyTuitionCentre);
                                           tuitionRepository.ClearSession();



                                           ExistingSession = new Domain.Session()
                                                                {
                                                                    Day = DayOfWeek.Thursday,
                                                                    From = new TimeSpan(11, 00, 00),
                                                                    To = new TimeSpan(13, 00, 00),
                                                                    Location = DummyTuitionCentre,
                                                                    SubjectsTaughtAtSession = new List<Subject>
                                                                                   {
                                                                                       new Subject
                                                                                           {
                                                                                             Name  = "Maths",
                                                                                             Level = Curriculum.Gcse
                                                                                           },
                                                                                           new Subject
                                                                                           {
                                                                                             Name  = "English",
                                                                                             Level = Curriculum.Gcse
                                                                                           }
                                                                                   }
                                                                };
                                           DataRepository.Save(ExistingSession);
                                       };
    }
}
