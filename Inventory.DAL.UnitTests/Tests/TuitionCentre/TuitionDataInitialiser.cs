using AECMIS.DAL.UnitTests.Helpers;
using Machine.Specifications;

namespace AECMIS.DAL.UnitTests.Tests.TuitionCentre
{
    public abstract class TuitionDataInitialiser : TestBase<Domain.TuitionCentre>
    {        
        protected static Domain.TuitionCentre DummyTuitionCentre;
        
        public new Establish context = () =>
                                           {

                                               DummyTuitionCentre = DbData.GetDummyTuitionCentre();                                               
                                               DataRepository.Save(DummyTuitionCentre);
                                               DataRepository.ClearSession();
                                           };
    }
}