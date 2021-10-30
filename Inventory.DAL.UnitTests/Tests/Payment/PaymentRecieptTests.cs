using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Criteria;
using AECMIS.DAL.UnitTests.Helpers;
using NUnit.Framework;

namespace AECMIS.DAL.UnitTests.Tests.Payment
{
    [TestFixture]
    public class PaymentRecieptTests:BaseTest<PaymentReciept>
    {
        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
        }

        public override void VerifyMapping()
        {
            throw new NotImplementedException();
        }

        
        //test to see i
        
       [Test]
        public void GetDebitableInvoices()
        {
           var s = new SqLiteRepository<Invoice, int>();
           var a = new SqLiteRepository<SessionAttendance, int>();
           var studentId = 1;
           //
            var n = s.FindAll().Where(x => x.Student.Id == studentId && x.PaymentReciept !=null &&  x.PaymentType == InvoicePaymentType.Advanced && x.DebitedSessions.Count < x.PaymentReciept.InvoicePayment.NumberOfSessionsPaidFor ).ToList();
           //

            var p = a.FindAll().Where(x => x.Student.Id == studentId && 
                (x.Status == SessionAttendanceStatus.Attended || x.Status == SessionAttendanceStatus.UnAuthorizedAbsence) 
                && x.Invoice.PaymentType == InvoicePaymentType.Advanced && x.Invoice.PaymentReciept == null).ToList();

           //var query = DetachedQuery.GetDebitableInvoices(1,Session);
           // var t = s.CreateCriteria(query);
        }
    }
}
