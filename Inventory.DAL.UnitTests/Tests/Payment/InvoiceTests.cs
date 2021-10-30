using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.UnitTests.Helpers;
using FizzWare.NBuilder;
using FluentNHibernate.Testing;
using NUnit.Framework;
using System.Linq;

namespace AECMIS.DAL.UnitTests.Tests.Payment
{
    [TestFixture]
    public class InvoiceTests:BaseTest<Invoice>
    {
        private SqLiteRepository<InvoicePayment, int> _invoicePaymentRepository;
            
        [OneTimeSetUp]
        public new void SetUp()
        {
            base.SetUp();
            _invoicePaymentRepository = new SqLiteRepository<InvoicePayment, int>();
        }

        [Test]
        public override void VerifyMapping()
        {
            var comparer = new EqualityComparer();
            comparer.RegisterComparer((Domain.Student x) => x.Id);
            comparer.RegisterComparer((Domain.PaymentReciept x) => x.Id);


            var centre = DbData.PopulateTuitionCentres().First();
            var session = DbData.PopulateSession(centre);
            var subjects = DbData.PopulateSubjects();            
            var paymentplan = DbData.PopulatePaymentPlan();
            var student = DbData.PopulateStudents(null,session, paymentplan,1, subjects).Build().First();

            new PersistenceSpecification<Invoice>(Session, comparer).
                CheckProperty(x => x.DateOfGeneration, new DateTime(2012, 12, 1, 11, 00, 00)).
                CheckProperty(x => x.DiscountApplied, (decimal) 20).
                CheckProperty(x => x.TotalAmount, (decimal) 150).
                CheckProperty(x => x.TotalAfterDiscount, (decimal) 130).
                CheckProperty(x => x.NumberOfSessionsPayingFor, 6).
                CheckReference(x => x.Student, student).
                //CheckReference(x => x.PaymentReciept, new PaymentReciept
                //                                          {
                //                                              AmountPaid = 100,
                //                                              NumberOfSessionsPaidFor = 5,
                //                                              PaymentDate = new DateTime(2012, 12, 15),

                //                                          }).
                VerifyTheMappings();
        }

        [Test]
        public void CanAddPayment()
        {
            var centre = DbData.PopulateTuitionCentres().First();
            var session = DbData.PopulateSession(centre);
            var subjects = DbData.PopulateSubjects();
            var paymentplan = DbData.PopulatePaymentPlan();
            var student = DbData.PopulateAndPersistStudents(null,session,paymentplan, 1,subjects).First();
            
            var payment = new InvoicePayment
                              {
                                  PaymentAmount = 100,
                                  NumberOfSessionsPaidFor = 5,
                                  PaymentDate = new DateTime(2012, 12, 15),
                                  
                              };

           

            
            var invoice = new Invoice
                              {
                                  DateOfGeneration = new DateTime(2012, 12, 1, 11, 00, 00),
                                  DiscountApplied = (decimal) 20,
                                  TotalAmount = (decimal) 150,
                                  TotalAfterDiscount = (decimal) 130,
                                  NumberOfSessionsPayingFor = 6,
                                  Student = student                                 
                              };

            var receipt = new PaymentReciept
            {
                InvoicePayment = payment,
                GeneratedDate = DateTime.Now,
                Invoice = invoice
            };
            
            //
            
            Session.SaveOrUpdate(invoice);
            payment.Invoice = invoice;
            
            
            _invoicePaymentRepository.Save(payment);
            invoice.PaymentReciept = receipt;
            receipt.InvoicePayment = payment;
            Session.Clear();
            Session.SaveOrUpdate(invoice);
            
            Assert.IsTrue(invoice.Id > 0 && payment.Id > 0 );            

        }

        [Test]
        public void DebitInvoiceWithSameNumberOfSessionsAsPaidFor()
        {
            const int numberofPayedSession = 2;
            var invoice = GetDummyInvoice();
            var payment = GetDummyPayment(invoice, numberofPayedSession);
            var attendences = new List<SessionAttendance>
                                  {
                                      _getAttendance(new DateTime(2012, 11, 19)),
                                      _getAttendance(new DateTime(2012, 11, 19))
                                  };
            invoice.PaymentReciept = payment;

            //act
            invoice.DebitInvoiceWithAttendences(attendences);

            //assert
            Assert.AreEqual(invoice.DebitedSessions.Count, attendences.Count);
        }
 
            
        [Test]
        public void DebitInvoicesWithLessSessionsThanPaidFor()
        {
            const int numberofPayedSession = 2;
            var invoice = GetDummyInvoice();
            var payment = GetDummyPayment(invoice, numberofPayedSession);
            var attendences = new List<SessionAttendance>
                                  {
                                      _getAttendance(new DateTime(2012, 11, 19))                                      
                                  };
            invoice.PaymentReciept = payment;

            //act
            invoice.DebitInvoiceWithAttendences(attendences);

            //assert
            Assert.AreEqual(invoice.DebitedSessions.Count, attendences.Count);
        }
        

        [Test]
        public void DebitInvoiceWithMoreSessionsThanPaidFor()
        {
            const int numberofPayedSession = 2;
            var invoice = GetDummyInvoice();
            var payment = GetDummyPayment(invoice, numberofPayedSession);
            var attendences = new List<SessionAttendance>
                                  {
                                      _getAttendance(new DateTime(2012, 11, 19)),
                                      _getAttendance(new DateTime(2012, 11, 20)),
                                      _getAttendance(new DateTime(2012, 11, 21))                                      
                                  };
            invoice.PaymentReciept = payment;

            //act
            invoice.DebitInvoiceWithAttendences(attendences);

            //assert
            Assert.AreEqual(1,attendences.Count(x => x.Invoice == null));
        }


        private static Invoice GetDummyInvoice()
        {
            return new Invoice
                       {
                           DateOfGeneration = new DateTime(2012, 11, 15),
                           DiscountApplied = 10,
                           NumberOfSessionsPayingFor = 5,
                           PaymentType = InvoicePaymentType.Advanced,
                           TotalAfterDiscount = 150
                       };
        }
        private static PaymentReciept GetDummyPayment(Invoice invoice, int numberOfSessionsPayedFor)
        {
            return new PaymentReciept
                       {                           
                           InvoicePayment = new InvoicePayment(){NumberOfSessionsPaidFor = numberOfSessionsPayedFor, PaymentAmount = 46},
                           GeneratedDate = new DateTime(2012, 12, 18),
                           Invoice = invoice
                       };
        }

        private readonly Func<DateTime, SessionAttendance> _getAttendance = date => new SessionAttendance
        {
            SessionRegister = new Domain.SessionRegister{Date = date, Session = Builder<Domain.Session>.CreateNew().Build()},
            Student =
                Builder<Domain.Student>.CreateNew().
                Build(),
            SubjectsAttended =
                Builder<SubjectAttendance>.
                CreateListOfSize(1).Build(),
            Status = SessionAttendanceStatus.Attended
        };  


    }
}
