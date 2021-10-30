using FluentNHibernate.Testing;
using AECMIS.DAL.UnitTests.Tests.Session;
using NUnit.Framework;

namespace AECMIS.DAL.UnitTests.Tests.TuitionCentre
{
    [TestFixture]
    public class TuitionTests:BaseTest<Domain.TuitionCentre>
    {
        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
        }
        
        [Test]
        public override void VerifyMapping()
        {            
            new PersistenceSpecification<Domain.TuitionCentre>(Session).
                CheckProperty(x => x.Address, "test address").
                CheckProperty(x => x.Name, "test name").
                VerifyTheMappings();
        }
    }


}
