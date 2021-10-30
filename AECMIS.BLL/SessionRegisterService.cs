using System;
using System.Collections.Generic;
using System.Transactions;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Domain.Extensions;
using AECMIS.DAL.Nhibernate;
using AECMIS.DAL.Nhibernate.Repositories;
using System.Linq;
using AutoMapper;
using AECMIS.Service.Comparers;
using NHibernate;
using NHibernate.Linq;
using IsolationLevel = System.Data.IsolationLevel;

namespace AECMIS.Service
{
    public class SessionRegisterService
    {
        private readonly IRepository<Session, int> _sessionRepository;
        private readonly IRepository<Student, int> _studentRepository;
        private readonly SessionAttendanceRepository _sessionAttendanceRepository;
       // private readonly IRepository<SessionAttendance,int> _sessionAttendanceRepository;
        private readonly IRepository<PaymentPlan, int> _paymentPlanRepository;
        private readonly IRepository<Teacher, int> _teacherRepository;
        private readonly IRepository<TuitionCentre, int> _tuitionCentreRepository;
        private readonly IRepository<SessionRegister, int> _sessionRegisterRepository;

        private readonly ISession Session;

       
        private readonly DateTime _dateToSearchExistingRegistersFrom = new DateTime(2012,01,01);

        //Changes added -- to get unpaid invoices using payment receipt
        private static readonly Func<Student, Invoice> GetRecentUnpaidInvoice = student =>
            student.Invoices.Where(x => x.PaymentReciept == null).
                OrderByDescending(x => x.DateOfGeneration).FirstOrDefault();
        //student.Invoices.Where(x => x.PaymentType == InvoicePaymentType.Advanced && x.PaymentReciept == null).
        //    OrderByDescending(x => x.DateOfGeneration).FirstOrDefault();

        private static readonly Func<Student, List<Invoice>> GetOutstandingDailyInvoices = student =>
            student.Invoices.Where(x => x.PaymentType == InvoicePaymentType.Daily && x.PaymentReciept == null).
                OrderByDescending(x => x.DateOfGeneration).ToList();
        private static readonly Func<Student, List<Invoice>> GetUnpaidInvoices = student =>
                    student.Invoices.Where(x => x.PaymentReciept == null && x.Status != InvoiceStatus.Cancelled).
                        OrderByDescending(x => x.DateOfGeneration).ToList();

        private static readonly Func<Student, List<Invoice>> GetPaidDebitableInvoices =
            student => student.Invoices.Where(i => i.PaymentReciept != null && i.Status != InvoiceStatus.Cancelled &&
                                                   i.PaymentType == InvoicePaymentType.Advanced &&
                                                  i.DebitedSessions.Count(s=>s.Status!= SessionAttendanceStatus.AuthorizeAbsence) < i.PaymentReciept.InvoicePayment.NumberOfSessionsPaidFor).
                                                   OrderBy(x => x.DateOfGeneration).ToList();


        private readonly Func<SessionAttendance,Student, bool> _attendanceIsChargeable =
            (attendance, student) => attendance.Invoice == null && (attendance.Status == SessionAttendanceStatus.Attended ||
                          attendance.Status == SessionAttendanceStatus.UnAuthorizedAbsence || student.DefaultPaymentPlan.IsDiscountedPlan );    


        private readonly Func<List<DiscountedPaymentAmount>, Student, int?> _getRequiredPaymentId =
        (list, student) => GetRecentUnpaidInvoice(student) != null && RequiredPayment(list, student) != null ? RequiredPayment(list, student).Id : (int?)null;

        private static readonly Func<List<DiscountedPaymentAmount>, Student, DiscountedPaymentAmount> RequiredPayment =
            (list, student) => list.FirstOrDefault(x => x.Id == student.DefaultPaymentPlan.Id);

        private readonly Func<Student, List<PaymentPlan>, List<PaymentPlan>> _studentPaymentPlans =
           (student, list) => list.Where(x => x.Curriculum == student.Curriculum).ToList();

        private readonly Func<List<Student>, List<Curriculum>> _studentCurriculums =
       list => list.Select(x => x.Curriculum).Distinct().ToList(); 
         
        //private static readonly Func<IRepository<>  List<Student>, SessionAttendance, int, DateTime, bool> GetSavedAttendances =
        //    (list, sa, sessionId, date) => list.Select(s => s.Id).Contains(sa.Student.Id) && sa.Session.Id == sessionId &&
        //                                   sa.Date == date;
      private readonly Func<Student, List<SessionSubjectAttendanceModel>, SessionAttendance>
            _getExistingAttendanceForStudent = (student, list) => student.SessionAttendances.FirstOrDefault(x => x.Id == list.Select(s => s.AttendanceId).First());

        private static readonly Func<Student, Session, Subject> GetSubjectStudiedAtSession =
            (student, session) => student.Subjects.FirstOrDefault(x => x.Session.Id == session.Id)!=null?
                student.Subjects.Where(x => x.Session.Id == session.Id).Select(x => x.Subject).
                                      FirstOrDefault():null;

        private readonly Func<Student, Session, int?> _getSubjectStudiedAtSessionId =
            (student, session) => GetSubjectStudiedAtSession(student, session) != null
                ? GetSubjectStudiedAtSession(student, session).Id
                : (int?) null;

        private readonly Func<List<StudentNoteDto>, Student, string> _getPreviousNotes = (notes, student) =>
            notes.Count > 0 && notes.Any(x => x.StudentId == student.Id) ? 
            notes.Where(x => x.StudentId == student.Id).OrderByDescending(x => x.Date).
            Select(p => string.Format("{0}-{1}\r\n\r\n", p.Date.ToString("yyyy-MM-dd"), p.Note)).Aggregate((i, o) => i + o) : "";

        //Changes added -- get available credits
        private readonly Func<Student, bool> _creditsAreAvailable =
           (student) => student.AttendanceCredits() != null && student.AttendanceCredits().Count(x => x.Attendance == null) > 0;

        //Changes added -- to check whether payment required
        private readonly Func<Student, bool> _paymentRequired=
             (student) => student.AttendanceCredits() != null && student.AttendanceCredits().Where(x => x.Attendance == null).Count() < 1;
        public SessionRegisterService():this(null,null,null,null,null,null,null, null)
        {
        }

        public SessionRegisterService(ISession nSession, IRepository<Session, int> sessionRep, IRepository<Student, int> studentRep,
            SessionAttendanceRepository sessionAttendanceRep, IRepository<PaymentPlan, int> paymentPlanRep, 
            IRepository<SessionRegister, int> sessionRegisterRep, IRepository<Teacher,int> teacherRepository, IRepository<TuitionCentre, int> tuitionCentreRepo)
        {
            Session = nSession?? SessionManager.GetSession();
            _sessionRepository = sessionRep ?? new Repository<Session, int>();
            _studentRepository = studentRep ?? new Repository<Student, int>();
            //_sessionAttendanceRepository = sessionAttendanceRep ?? new Repository<SessionAttendance, int>();
           _sessionAttendanceRepository = sessionAttendanceRep ?? new SessionAttendanceRepository();
            _paymentPlanRepository = paymentPlanRep ?? new Repository<PaymentPlan, int>();
            _teacherRepository = teacherRepository??new Repository<Teacher, int>();
            _sessionRegisterRepository = sessionRegisterRep ?? new Repository<SessionRegister, int>();
            _tuitionCentreRepository = tuitionCentreRepo ?? new Repository<TuitionCentre, int>();
        }

        //An Advance payment in this system is student pays for today + next ndays
        //if a student has an unauthorized absence on the day of payment they must be charged for the session and the advanced invoice must
        //be converted to a daily one and assigned to that session the system must then generate a new advanced invoice
        internal void ProcessStudentRegisters(List<int> studentsIds)
        {
            var students = Session.Query<Student>().Where(x => studentsIds.Contains(x.Id));

            students.ToList().ForEach(student =>
                                 {
                                     
                                     var unpaidAttendances =
                                         student.SessionAttendances.ToList().Where(
                                             a => _attendanceIsChargeable(a, student)).ToList();

                                     ProcessOutstandingDailyPayments(student);
                                     //process payment
                                     ProcessPayment(unpaidAttendances, student);

                                     //debit invoices
                                     DebitPaidInvoices(student, unpaidAttendances);

                                     //Changes added -- Commented to stop generating future invoices

                                     //Check if new invoice needs to be created
                                     //CheckForNewInvoiceCreation(student, unpaidAttendances);
                                 });
        }


        public void ChangeAttendanceStatusChargeToNonCharge(int sessionAttendanceId, int invoiceId, string CreditNotes)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var sessionattendanceObj = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();

                Invoice invalidate = Session.Query<Invoice>().Where(x => x.Id == invoiceId).First();
                invalidate.Status = InvoiceStatus.Cancelled;
                sessionattendanceObj.Status = SessionAttendanceStatus.AuthorizeAbsence;
                Session.Flush();
                Session.Update(sessionattendanceObj);
                Session.Update(invalidate);
                CreditNote creditNote = new CreditNote();
                creditNote.Notes = CreditNotes; creditNote.Invoice = invalidate;
                creditNote.CreatedDate = DateTime.Now;
                creditNote = CalculateCreditNoteSequenceNo(creditNote);
                Session.Flush();
                Session.SaveOrUpdate(creditNote);
                tx.Commit();
            }

        }

        public CreditNote CalculateCreditNoteSequenceNo(CreditNote creditNote)
        {
            var CreditNoteSequencing = Session.Query<CreditNoteSequencing>().Where(x => creditNote.CreatedDate <= x.ToDate && creditNote.CreatedDate >= x.FromDate).First();
            var creditnotes = Session.Query<CreditNote>().Where(x => x.ModifiedDate <= CreditNoteSequencing.ToDate && x.ModifiedDate >= CreditNoteSequencing.FromDate).ToList();
            int count = creditnotes.Count();
            creditNote.CreditNoteRefNumber = CreditNoteSequencing.SequenceStartNum + count + 1;
            return creditNote;
        }

        public void ChangeAttendanceStatusNonChargeableToChargeble(int sessionAttendanceId, int InvoiceId, int NewStatus, string StudentNo, string PaymentPlan, string PaymentType,
            string ChequeNo, string PaymentDate, string IsDailyInvoiceData, int FutureInvoiceSelect)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                if ((IsDailyInvoiceData == "true" || (!string.IsNullOrEmpty(PaymentType) && PaymentType != "0")) && FutureInvoiceSelect == 0)
                {
                    InvoiceId = ProcessCreateNewInvoice(StudentNo, sessionAttendanceId, PaymentPlan, PaymentType, ChequeNo, PaymentDate, IsDailyInvoiceData);
                }
                else
                    InvoiceId = FutureInvoiceSelect;
                InvoiceId = CreateNewInvoiceForAdvancedNonChargableToChargable(sessionAttendanceId, InvoiceId);
                var sessionattendanceObj = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();

                var invoice = Session.Query<Invoice>().Where(x => x.Id == InvoiceId).First();
                if (invoice.NumberOfSessionsPayingFor > 1)
                {
                    var paymentreceipt = Session.Query<PaymentReciept>().Where(x => x.Id == invoice.PaymentReciept.Id).First();
                    var attendancecredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == paymentreceipt.Id && x.Attendance == null).OrderBy(x => x.CreatedDate).First();
                    attendancecredit.Receipt = paymentreceipt;
                    attendancecredit.Attendance = sessionattendanceObj;
                    Session.Flush();
                    Session.SaveOrUpdate(attendancecredit);
                }
                sessionattendanceObj.Invoice = invoice;
                sessionattendanceObj.Status = NewStatus == 1001 ? SessionAttendanceStatus.Attended : SessionAttendanceStatus.UnAuthorizedAbsence;
                Session.Flush();
                Session.SaveOrUpdate(sessionattendanceObj);
                tx.Commit();
            }
        }

        public int CreateNewInvoiceForAdvancedNonChargableToChargable(int sessionAttendanceId, int InvoiceId)
        {
            if (InvoiceId > 0)
                return InvoiceId;
            var sessionAttended = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();
            var invoicePayment = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == sessionAttended.Id).FirstOrDefault();
            if(invoicePayment == null)
            {
                var student = Session.Query<Student>().Where(x => x.Id == sessionAttended.Student.Id).First();
                var invoices = Session.Query<Invoice>().Where(x => x.Student.Id == student.Id).ToList();
                foreach (var inv in invoices)
                {
                    var invoicereceipt = Session.Query<PaymentReciept>().Where(x => x.Invoice.Id == inv.Id).FirstOrDefault();
                    if (invoicereceipt != null)
                    {
                        var attCredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invoicereceipt.Id && x.Attendance == null).ToList();
                        if (attCredit.Count > 0)
                        {
                            return inv.Id;
                        }
                        else
                        {
                            continue;
                        }

                    }
                }
            }
            int invoiceid = 0;
            if(invoicePayment.NumberOfSessionsPaidFor > 1)
            {
                Invoice invoice;
                SaveNewInvoiceForAdvancedNonChargeableToChargeable(sessionAttended, invoicePayment, out invoiceid, out invoice);
                SavePaymentReceiptAndAttendanceCreditForAdvanceNonChargabletoChargeable(invoicePayment, invoice);

            }
            return invoiceid;
        }

        private void SavePaymentReceiptAndAttendanceCreditForAdvanceNonChargabletoChargeable(InvoicePayment invoicePayment, Invoice invoice)
        {
            PaymentReciept reciept = new PaymentReciept();
            reciept.GeneratedDate = DateTime.Now;
            reciept.Invoice = invoice; reciept.InvoicePayment = invoicePayment;
            Session.Flush();
            Session.Save(reciept);
            for (int i = 0; i < invoicePayment.NumberOfSessionsPaidFor; i++)
            {
                AttendanceCredit credit = new AttendanceCredit();
                credit.Receipt = reciept;
                Session.Flush();
                Session.Save(credit);
            }

            invoice.PaymentReciept = reciept;
            Session.Flush();
            Session.SaveOrUpdate(invoice);

            invoicePayment.Invoice = invoice; invoicePayment.Reciept = reciept;
            Session.Flush();
            Session.SaveOrUpdate(invoicePayment);
        }

        private void SaveNewInvoiceForAdvancedNonChargeableToChargeable(SessionAttendance sessionAttended, InvoicePayment invoicePayment, out int invoiceid, out Invoice invoice)
        {
            invoice = new Invoice();
            invoice.NumberOfSessionsPayingFor = invoicePayment.NumberOfSessionsPaidFor;
            invoice.DiscountApplied = Convert.ToDecimal(0);

            invoice.DateOfGeneration = DateTime.Now;

            invoice.Status = InvoiceStatus.Paid;
            invoice.Student = sessionAttended.Student;
            invoice.TotalAmount = invoicePayment.PaymentAmount.Value;
            invoice.PaymentType = invoicePayment.NumberOfSessionsPaidFor > 1 ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            invoice.TotalAfterDiscount = invoice.TotalAmount - invoice.DiscountApplied.Value;
            invoice = CalculateVATAndSequenceNumber(invoice);
            invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            Session.Flush();
            invoiceid = (int)Session.Save(invoice);
        }

        public void ChangeAttendanceStatusChargebleToNonChargable(int sessionAttendanceId)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var sessionattendanceObj = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();
                var attendancecreditObj = Session.Query<AttendanceCredit>().Where(x => x.Attendance.Id == sessionAttendanceId).FirstOrDefault();
                sessionattendanceObj.Status = SessionAttendanceStatus.AuthorizeAbsence;
                if(attendancecreditObj != null)
                    attendancecreditObj.Attendance = null;
                Session.Flush();
                Session.SaveOrUpdate(sessionattendanceObj);
                if(attendancecreditObj != null)
                    Session.SaveOrUpdate(attendancecreditObj);
                tx.Commit();
            }
        }

        public Invoice CalculateVATAndSequenceNumber(Invoice oldinvoice)
        {
            var invoiceSequencing = Session.Query<InvoiceSequencing>().FirstOrDefault(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate);
            var objVATDetails = Session.Query<VATDetails>().FirstOrDefault(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate);
            var invoices = Session.Query<Invoice>().Where(x => x.DateOfGeneration <= objVATDetails.ToDate && x.DateOfGeneration >= objVATDetails.FromDate).ToList();
            var vatPercentage = objVATDetails?.VATPercentage ?? 20;
            int count = invoices.Count();
            if (oldinvoice.Id > 0)
            {
                count = invoices.Count(x => x.Id < oldinvoice.Id);
            }
            oldinvoice.InvoiceRefNumber = invoiceSequencing.SequenceStartNum + count + 1;
            oldinvoice.VATAmount =  Math.Round(oldinvoice.TotalAfterDiscount - ((oldinvoice.TotalAfterDiscount / (100m + vatPercentage)) * 100m), 2);
            oldinvoice.TotalExcludeVAT = Math.Round(oldinvoice.TotalAfterDiscount - oldinvoice.VATAmount,2);
            return oldinvoice;
        }

        public void ChangeAttendanceStatusOnly(int sessionAttendanceId)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var sessionattendanceObj = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();
                var attendanceCredits = Session.Query<AttendanceCredit>().Where(x => x.Attendance.Id == sessionAttendanceId).FirstOrDefault();
                if (attendanceCredits != null)
                {
                    AttendanceCredit attendanceCredit = attendanceCredits;
                    attendanceCredit.Attendance = null;
                    Session.Update(attendanceCredit);

                }
                tx.Commit();
            }
        }

        public void ChargeToChargable(int sessionAttendanceId, int NewStatus)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var sessionattendanceObj = Session.Query<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).First();
                sessionattendanceObj.Status = NewStatus == 1001 ? SessionAttendanceStatus.Attended : SessionAttendanceStatus.UnAuthorizedAbsence;
                Session.Flush();
                Session.Update(sessionattendanceObj);
                tx.Commit();
            }
        }

        public int ProcessNewInvoiceStudent(int StudentId, string PaymentPlan, string PaymentTypedata, string ChequeNo, string PaymentDate,string Discount, decimal VATAmount)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var paymentplan = Session.Query<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(PaymentPlan)).First();
                var student = Session.Query<Student>().Where(x => x.Id == StudentId).First();
                decimal discountValue = !string.IsNullOrEmpty(Discount) ? Convert.ToDecimal(Discount) : Convert.ToDecimal(0);

                var invoicePayment = new InvoicePayment();
                var paymenttype = (PaymentType)System.Enum.Parse(typeof(PaymentType), PaymentTypedata);

                invoicePayment = AddInvoicePaymentToProcessNewInvoiceWOSessionAttendance(ChequeNo, PaymentDate, paymentplan, paymenttype, discountValue);
                Invoice invoice;
                int invoiceid;
                CreateNewInvoiceForProcessingNewInvoice(ChequeNo, PaymentDate, Discount, VATAmount, paymentplan, student, invoicePayment, paymenttype, out invoice, out invoiceid);

                PaymentReciept paymentReciept = new PaymentReciept();
                paymentReciept.Invoice = invoice;
                paymentReciept.InvoicePayment = invoicePayment;
                paymentReciept.GeneratedDate = DateTime.Now;
                Session.Flush();
                int paymentrecieptid = (int)Session.Save(paymentReciept);
                if (paymentplan.TotalSessions > 1)
                {
                    for (int i = 0; i < paymentplan.TotalSessions; i++)
                    {
                        AttendanceCredit credit = new AttendanceCredit();
                        credit.Receipt = paymentReciept;
                        Session.Flush();
                        Session.Save(credit);
                    }
                }
                invoicePayment.Invoice = invoice;
                Session.Flush();
                Session.SaveOrUpdate(invoicePayment);

                invoice.PaymentReciept = paymentReciept;
                Session.Flush();
                Session.SaveOrUpdate(invoice);

                tx.Commit();
                return invoiceid;
            }

        }

        private void CreateNewInvoiceForProcessingNewInvoice(string ChequeNo, string PaymentDate, string Discount, decimal VATAmount, PaymentPlan paymentplan, Student student, InvoicePayment invoicePayment, PaymentType paymenttype, out Invoice invoice, out int invoiceid)
        {
            invoice = new Invoice();
            invoice.NumberOfSessionsPayingFor = paymentplan.TotalSessions;
            if (!string.IsNullOrEmpty(Discount))
                invoice.DiscountApplied = Convert.ToDecimal(Discount);
            else
                invoice.DiscountApplied = Convert.ToDecimal(0);
            if (invoicePayment.PaymentType == PaymentType.None)
            {
                invoicePayment.PaymentDate = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : invoicePayment.PaymentDate;
                invoicePayment.PaymentType = paymenttype;
                invoicePayment.PaymentAmount = paymentplan.Amount;
                invoicePayment.ChequeNo = ChequeNo;
            }

            DateTime validatedatetime = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            bool isvaliddatetime = validatedatetime.Year > 1;

            invoice.DateOfGeneration = (!string.IsNullOrEmpty(PaymentDate) && isvaliddatetime) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            if (invoicePayment.PaymentType != PaymentType.None)
                invoice.Status = InvoiceStatus.Paid;
            else
                invoice.Status = InvoiceStatus.Pending;
            invoice.Student = student;
            invoice.TotalAmount = (decimal)paymentplan.Amount;
            invoice.PaymentType = paymentplan.TotalSessions > 1 ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            if (!string.IsNullOrEmpty(Discount))
                invoice.TotalAfterDiscount = invoice.TotalAmount - Convert.ToDecimal(Discount);
            else
                invoice.TotalAfterDiscount = invoice.TotalAmount;
            invoice = CalculateVATAndSequenceNumber(invoice);
            invoice.VATAmount = Convert.ToDecimal(VATAmount);
            invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            invoice.TotalExcludeVAT = invoice.TotalAfterDiscount - invoice.VATAmount;
            Session.Flush();
            invoiceid = (int)Session.Save(invoice);
        }

        private InvoicePayment AddInvoicePaymentToProcessNewInvoiceWOSessionAttendance(string ChequeNo, string PaymentDate, PaymentPlan paymentplan, PaymentType paymenttype, decimal discount)
        {
            InvoicePayment invoicePayment = new InvoicePayment();
            invoicePayment.NumberOfSessionsPaidFor = paymentplan.TotalSessions;

            invoicePayment.PaymentDate = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : invoicePayment.PaymentDate;

            invoicePayment.PaymentType = paymenttype;
            invoicePayment.PaymentAmount = paymentplan.Amount - discount;
            invoicePayment.ChequeNo = ChequeNo;
            Session.Flush();
            int paymentid = (int)Session.Save(invoicePayment);
            return invoicePayment;
        }

        public int ProcessNewInvoice(string StudentNo, int sessionAttendanceId, string PaymentPlan, string PaymentTypedata, string ChequeNo, string PaymentDate, string IsDailyInvoiceData)
        {
            using (var tx = this.Session.BeginTransaction())
            {
                var paymentplan = Session.Query<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(PaymentPlan)).First();
                var student = Session.Query<Student>().Where(x => x.StudentNo == StudentNo).First();
                var sessionAttendance = Session.QueryOver<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).List().First();
                var invoicePayment = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == sessionAttendanceId).FirstOrDefault();
                var paymenttype = (PaymentType)System.Enum.Parse(typeof(PaymentType), PaymentTypedata);
                if (invoicePayment == null)
                {
                    invoicePayment = AddInvoicePaymentToProcessNewInvoice(ChequeNo, PaymentDate, paymentplan, sessionAttendance, paymenttype);
                }
                Invoice invoice;
                int invoiceid;
                CreateAndSaveNewInvoice(ChequeNo, PaymentDate, paymentplan, student, invoicePayment, paymenttype, out invoice, out invoiceid);
                CreateAndSavePaymentReceiptAndAttendanceCredit(paymentplan, invoicePayment, invoice);

                tx.Commit();
                return invoiceid;
            }

        }

        private void CreateAndSavePaymentReceiptAndAttendanceCredit(PaymentPlan paymentplan, InvoicePayment invoicePayment, Invoice invoice)
        {
            PaymentReciept paymentReciept = new PaymentReciept();
            paymentReciept.Invoice = invoice;
            paymentReciept.InvoicePayment = invoicePayment;
            paymentReciept.GeneratedDate = DateTime.Now;
            Session.Flush();
            int paymentrecieptid = (int)Session.Save(paymentReciept);
            if (paymentplan.TotalSessions > 1)
            {
                for (int i = 0; i < paymentplan.TotalSessions; i++)
                {
                    AttendanceCredit credit = new AttendanceCredit();
                    credit.Receipt = paymentReciept;
                    Session.Flush();
                    Session.Save(credit);
                }
            }
            invoicePayment.Invoice = invoice;
            Session.Flush();
            Session.SaveOrUpdate(invoicePayment);

            invoice.PaymentReciept = paymentReciept;
            Session.Flush();
            Session.SaveOrUpdate(invoice);
        }

        private void CreateAndSaveNewInvoice(string ChequeNo, string PaymentDate, PaymentPlan paymentplan, Student student, InvoicePayment invoicePayment, PaymentType paymenttype, out Invoice invoice, out int invoiceid)
        {
            invoice = new Invoice();
            invoice.NumberOfSessionsPayingFor = paymentplan.TotalSessions;
            invoice.DiscountApplied = Convert.ToDecimal(0);
            if (invoicePayment.PaymentType == PaymentType.None)
            {
                invoicePayment.PaymentDate = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : invoicePayment.PaymentDate;
                invoicePayment.PaymentType = paymenttype;
                invoicePayment.PaymentAmount = paymentplan.Amount;
                invoicePayment.ChequeNo = ChequeNo;
            }
            DateTime validatedatetime = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            bool isvaliddatetime = validatedatetime.Year > 1;

            invoice.DateOfGeneration = (!string.IsNullOrEmpty(PaymentDate) && isvaliddatetime) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            if (invoicePayment.PaymentType != PaymentType.None)
                invoice.Status = InvoiceStatus.Paid;
            else
                invoice.Status = InvoiceStatus.Pending;
            invoice.Student = student; invoice.TotalAmount = (decimal)invoicePayment.PaymentAmount;
            invoice.PaymentType = paymentplan.TotalSessions > 1 ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            invoice.TotalAfterDiscount = invoice.TotalAmount - invoice.DiscountApplied.Value;
            invoice = CalculateVATAndSequenceNumber(invoice);
            invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            Session.Flush();
            invoiceid = (int)Session.Save(invoice);
        }

        private InvoicePayment AddInvoicePaymentToProcessNewInvoice(string ChequeNo, string PaymentDate, PaymentPlan paymentplan, SessionAttendance sessionAttendance, PaymentType paymenttype)
        {
            InvoicePayment invoicePayment = new InvoicePayment();
            invoicePayment.NumberOfSessionsPaidFor = paymentplan.TotalSessions;
            invoicePayment.Attendance = sessionAttendance;
            invoicePayment.PaymentDate = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : invoicePayment.PaymentDate;

            invoicePayment.PaymentType = paymenttype;
            invoicePayment.PaymentAmount = paymentplan.Amount;
            invoicePayment.ChequeNo = ChequeNo;
            Session.Flush();
            int paymentid = (int)Session.Save(invoicePayment);
            return invoicePayment;
        }

        public int ProcessCreateNewInvoice(string StudentNo, int sessionAttendanceId, string PaymentPlan, string PaymentTypedata, string ChequeNo, string PaymentDate, string IsDailyInvoiceData)
        {

            var paymentplan = Session.Query<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(PaymentPlan)).FirstOrDefault();
            var student = Session.Query<Student>().Where(x => x.StudentNo == StudentNo).First();
            var sessionAttendance = Session.QueryOver<SessionAttendance>().Where(x => x.Id == sessionAttendanceId).List().First();
            var invoicePayment = Session.Query<InvoicePayment>().FirstOrDefault(x => x.Attendance.Id == sessionAttendanceId && x.Invoice.Status != InvoiceStatus.Cancelled);
            var paymenttype = (PaymentType)System.Enum.Parse(typeof(PaymentType), PaymentTypedata);
            if (invoicePayment == null)
            {
                if (paymenttype == PaymentType.None)
                {
                    invoicePayment = new InvoicePayment();
                    invoicePayment.Attendance = sessionAttendance;
                    invoicePayment.PaymentType = paymenttype;
                }
                else
                    invoicePayment = AddInvoicePaymentToProcessNewInvoice(ChequeNo, PaymentDate, paymentplan, sessionAttendance, paymenttype);
            }
            Invoice invoice;
            int invoiceid;
            AddInvoice(ChequeNo, PaymentDate, paymentplan, student, invoicePayment, paymenttype, out invoice, out invoiceid);
            SavePaymentRecieptAndAttendanceCreditForProcessCreateNewInvoice(paymentplan, invoicePayment, invoice);

            return invoiceid;


        }

        private void SavePaymentRecieptAndAttendanceCreditForProcessCreateNewInvoice(PaymentPlan paymentplan, InvoicePayment invoicePayment, Invoice invoice)
        {
            PaymentReciept paymentReciept = new PaymentReciept();
            paymentReciept.Invoice = invoice;
            if (invoicePayment.PaymentType != PaymentType.None)
            {
                paymentReciept.InvoicePayment = invoicePayment;
                paymentReciept.GeneratedDate = DateTime.Now;
                Session.Flush();
                int paymentrecieptid = (int)Session.Save(paymentReciept);
            }
            if (paymentplan != null && paymentplan.TotalSessions > 1)
            {
                for (int i = 0; i < paymentplan.TotalSessions; i++)
                {
                    AttendanceCredit credit = new AttendanceCredit();
                    credit.Receipt = paymentReciept;
                    Session.Flush();
                    Session.Save(credit);
                }
            }
            invoicePayment.Invoice = invoice;
            Session.Flush();
            Session.SaveOrUpdate(invoicePayment);
            if (paymentplan != null && invoicePayment.PaymentType != PaymentType.None)
                invoice.PaymentReciept = paymentReciept;
            Session.Flush();
            Session.SaveOrUpdate(invoice);
        }

        private void UpdateInvoiceDetailsWhenPaymentIsNone(Invoice invoice, Student student)
        {
            var paymentPlans = Session.Query<PaymentPlan>().Where(x => student.Curriculum == x.Curriculum).ToList();
            var dailyRate =
                paymentPlans.FirstOrDefault(
                    x => x.Curriculum == student.DefaultPaymentPlan.Curriculum && x.TotalSessions == 1);

            if (dailyRate == null)
                throw new Exception(string.Format("Could not find daily rate for curriculum {0}",
                    student.DefaultPaymentPlan.Curriculum));
            invoice.TotalAmount = dailyRate.Amount;
            invoice.TotalAfterDiscount = dailyRate.Amount; //no discount for daily rate
            invoice.DiscountApplied = 0;
            invoice.NumberOfSessionsPayingFor = dailyRate.TotalSessions;
        }

        private void AddInvoice(string ChequeNo, string PaymentDate, PaymentPlan paymentplan, Student student, InvoicePayment invoicePayment, PaymentType paymenttype, out Invoice invoice, out int invoiceid)
        {
            invoice = new Invoice();
            invoice.NumberOfSessionsPayingFor = paymentplan != null ? paymentplan.TotalSessions : 0;
            invoice.DiscountApplied = Convert.ToDecimal(0);
            if (invoicePayment.PaymentType == PaymentType.None)
            {
                if(!string.IsNullOrEmpty(PaymentDate))
                    invoicePayment.PaymentDate = Convert.ToDateTime(PaymentDate);
                invoicePayment.PaymentType = paymenttype;
                invoicePayment.PaymentAmount = null;
                invoicePayment.ChequeNo = ChequeNo;
                UpdateInvoiceDetailsWhenPaymentIsNone(invoice, student);
            }

            DateTime validatedatetime = !string.IsNullOrEmpty(PaymentDate) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            bool isvaliddatetime = validatedatetime.Year > 1;

            invoice.DateOfGeneration = (!string.IsNullOrEmpty(PaymentDate) && isvaliddatetime) ? Convert.ToDateTime(PaymentDate) : DateTime.Now;
            invoice.Status = invoicePayment.PaymentType != PaymentType.None ? InvoiceStatus.Paid : InvoiceStatus.Pending;
            invoice.Student = student;
            invoice.TotalAmount = invoicePayment.PaymentAmount.HasValue ? (decimal)invoicePayment.PaymentAmount : invoice.TotalAmount;
            invoice.PaymentType = (paymentplan != null && paymentplan.TotalSessions > 1) ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            invoice.TotalAfterDiscount = invoice.TotalAmount;
            invoice = CalculateVATAndSequenceNumber(invoice);
            invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            Session.Flush();
            invoiceid = (int)Session.Save(invoice);
        }


        public SearchAttendanceResult GetAttendances(SearchStudentAttendanceCriteria searchCriteria)
        {
            return _sessionAttendanceRepository.GetAttendances(searchCriteria);
        }

        public StudentAttendanceDetails GetStudentAttendanceDetails(int id)
        {
            return _sessionAttendanceRepository.GetStudentAttendance(id);
        }

        public List<StudentAttendanceDetails> GetStudentAttendanceInvoices(int invoiceid)
        {
            return _sessionAttendanceRepository.GetStudentAttendanceInvoice(invoiceid);
        }



        internal void ProcessRegisterAttendances(List<SessionAttendance> sessionAttendances)
        {
            var studentIds = sessionAttendances.Select(x => x.Student.Id).ToList();
            var students = Session.Query<Student>().Where(x => studentIds.Contains(x.Id));

            students.ToList().ForEach(student =>
            {

                var currentRegistersAttendances =
                    sessionAttendances.Where(x => x.Student.Id == student.Id && _attendanceIsChargeable(x, student)).ToList();
                
                ProcessOutstandingDailyPaymentsForAttendances(student, currentRegistersAttendances);
                
                //process payment
                ProcessPayment(currentRegistersAttendances, student);

                //debit invoices
                DebitPaidInvoices(student, currentRegistersAttendances);

                //Changes added -- Commented to stop generating future invoices

                //Check if new invoice needs to be created
                //CheckForNewInvoiceCreation(student, currentRegistersAttendances);
            });
        }


        /// <summary>
        /// Processes daily invoices that may have been paid in these attendances
        /// </summary>
        /// <param name="student"></param>
        /// <param name="attendances"></param>
        internal void ProcessOutstandingDailyPaymentsForAttendances(Student student, List<SessionAttendance> attendances)
        {
            var outstandingInvoices = GetOutstandingDailyInvoices(student);
            if (outstandingInvoices == null || outstandingInvoices.Count < 1) return;

            var invoiceIds = outstandingInvoices.Select(x => x.Id).ToList();
            //Changes added -- get attendance Ids of outstanding invoices
            var attendanceIds = attendances.Where(x => x.Status == SessionAttendanceStatus.Attended || x.Status==SessionAttendanceStatus.UnAuthorizedAbsence).Select(x => x.Id).ToList();
           

            //only pick up a payment of a student who has attended, can non attending students pay??
            var payments = Session.Query<InvoicePayment>().Where(x => invoiceIds.Contains(x.Invoice.Id) &&
                                                              (x.PaymentType != PaymentType.None) &&
                                                              (attendanceIds.Contains(x.Attendance.Id)));
            outstandingInvoices.ForEach(x =>
            {
                var payment = payments.FirstOrDefault(p => p.Invoice.Id == x.Id);
                if (payment == null) return;
                x.ProcessPayment(payment);
            });

            Session.SaveOrUpdate(student);
            Session.Flush();
        }

        internal void ProcessOutstandingDailyPayments(Student student)
        {
            var outstandingInvoices = GetOutstandingDailyInvoices(student);
            if (outstandingInvoices == null || outstandingInvoices.Count < 1) return;            
            
            var invoiceIds = outstandingInvoices.Select(x => x.Id).ToList();

            //only pick up a payment of a student who has attended, can non attending students pay??
            var payments = Session.Query<InvoicePayment>().Where(x => invoiceIds.Contains(x.Invoice.Id) &&
                                                              (x.PaymentType != PaymentType.None) &&
                                                              (x.Attendance.Status == SessionAttendanceStatus.Attended));                
            outstandingInvoices.ForEach(x=>
                                            {
                                                var payment = payments.FirstOrDefault(p => p.Invoice.Id == x.Id);
                                                if (payment == null) return;
                                                x.ProcessPayment(payment);
                                            });

            Session.SaveOrUpdate(student);
            Session.Flush();           
        }

        //Changes added in below method
        internal void ProcessPayment(List<SessionAttendance> unpaidAttendances, Student student)
        {
            //check available credits
            if (_creditsAreAvailable(student)) return;

           
            //get last unpaid attendence and check if payment has been made
            var latestAttendance = unpaidAttendances.OrderByDescending(x => x.SessionRegister.Date).FirstOrDefault();
            if (latestAttendance == null) return;
            
            //var unpaidInvoice = GetRecentUnpaidInvoice(student);

            //if there is no unpaid invoice no need to process any further
            //if (unpaidInvoice == null) return;
            //if student does not have any unpaid attendances then exit
            if (!unpaidAttendances.Any()) return;

            var unpaidInvoice = CreateNewInvoice(student);
            var latestAttendancePayment = latestAttendance.Payments?.FirstOrDefault(x => x.Invoice == null);

            var paymentPlans = Session.Query<PaymentPlan>().Where(x => student.Curriculum == x.Curriculum).ToList();
            //if payment type is none i.e deferred
            //convert invoice to daily invoice
            // if (latestAttendance.PaymentForFutureInvoice == null || latestAttendance.PaymentForFutureInvoice.PaymentType == PaymentType.None || latestAttendance.Status == SessionAttendanceStatus.UnAuthorizedAbsence)
            if (latestAttendance.Payments?.Count==0 || latestAttendancePayment?.PaymentType == PaymentType.None|| latestAttendancePayment?.NumberOfSessionsPaidFor <= 1 || latestAttendance.Status == SessionAttendanceStatus.UnAuthorizedAbsence)
            {
                ProcessUnpaidInvoice(unpaidInvoice, paymentPlans, latestAttendance, latestAttendancePayment);
            }
            else
            {
                ProcessPaidInvoice(student, latestAttendance, paymentPlans, unpaidInvoice);
            }
           
            Session.SaveOrUpdate(student);
            unpaidInvoice = CalculateVATAndSequenceNumber(unpaidInvoice);
            Session.SaveOrUpdate(unpaidInvoice);
           
            //Changes added -- Update student invoice collection
            if (student.Invoices != null && student.Invoices.All(x => x.Id != unpaidInvoice.Id))
            {
                student.Invoices.Add(unpaidInvoice);
            }

            if (latestAttendancePayment!=null)
            {
                latestAttendancePayment.Invoice = unpaidInvoice;
                Session.SaveOrUpdate(latestAttendancePayment);
            }
            Session.Flush();
        }

        private void ProcessPaidInvoice(Student student, SessionAttendance latestAttendance, List<PaymentPlan> paymentPlans,
            Invoice unpaidInvoice)
        {
            //if there is a value but it is not equal to the price plan 
            //when discount was agreed then do not apply discount
            //Changes added -- Get payment plan from current attendance payment
            var invoicePayment = latestAttendance.Payments?.FirstOrDefault(x => x.Invoice == null);
            var paidPaymentPlan = paymentPlans.FirstOrDefault(x =>
                x.Id == latestAttendance.Payments.OrderByDescending(y => y.PaymentDate).FirstOrDefault()
                    ?.PaymentAmountId);
            if (invoicePayment != null)
                paidPaymentPlan = paymentPlans.FirstOrDefault(x => x.Id == invoicePayment.PaymentAmountId);
            //var paidPaymentPlan = paymentPlans.FirstOrDefault(x => x.Id == latestAttendance.PaymentForFutureInvoice.PaymentAmountId);
            if (paidPaymentPlan != null && paidPaymentPlan.TotalSessions < 2)
                unpaidInvoice.PaymentType = InvoicePaymentType.Daily;
            //amount paid
            //default payment plan
            unpaidInvoice.ProcessPayment(paidPaymentPlan, student.DefaultPaymentPlan, invoicePayment, student.DiscountAmount);

            //Changes added -- update attendance credits
            unpaidInvoice.PaymentReciept.AttendanceCredits =
                CreateAttendanceCreditsIfPaymentTypeIsFuture(invoicePayment, unpaidInvoice.PaymentReciept);
        }

        private void ProcessUnpaidInvoice(Invoice unpaidInvoice, List<PaymentPlan> paymentPlans, SessionAttendance latestAttendance,
            InvoicePayment latestAttendancePayment)
        {
            unpaidInvoice.ConvertUnpaidFutureInvoiceToDailyRateInvoice(paymentPlans);
            latestAttendance.Invoice = unpaidInvoice;
            //Changes added -- Generate Receipt for daily invoice if the latest attendance has been paid
            if (latestAttendance.Payments?.Count > 0 && latestAttendancePayment?.PaymentType != PaymentType.None)
            {
                unpaidInvoice.PaymentType = InvoicePaymentType.Daily;
                var invoicePayment = latestAttendance.Payments.OrderByDescending(y => y.PaymentDate)
                    .FirstOrDefault();

                unpaidInvoice.ProcessPayment(invoicePayment);
            }
        }

        ////Changes added
        private IList<AttendanceCredit> CreateAttendanceCreditsIfPaymentTypeIsFuture(InvoicePayment payment,
            PaymentReciept paymentReciept)
        {
            if (payment.NumberOfSessionsPaidFor > 1)
                return Enumerable.Range(0, payment.NumberOfSessionsPaidFor)
                    .Select(x => new AttendanceCredit { Receipt = paymentReciept }).ToList();
            return null;
        }




        public void ProcessRegister(SessionRegisterViewModel viewModel, int userId)
        {
            DateTime sessionDate;

            ValidateRegister(viewModel, out sessionDate);
            var classSessionData = PopulateSessionData(viewModel, sessionDate);

            using (var tx = Session.BeginTransaction(IsolationLevel.Serializable))
            {
                SaveAttendances(classSessionData.Students, classSessionData.SessionSubjectAttendances,
                            classSessionData.SessionRegister, classSessionData.Teachers,
                            classSessionData.ClassSession, sessionDate);
                Session.Flush();
                Session.Clear();
            
                var register = Session.Get<SessionRegister>(classSessionData.SessionRegister.Id);
                ProcessRegisterAttendances(register.SessionAttendances.ToList());
                //ProcessStudentRegisters(register.SessionAttendances.Select(x => x.Student.Id).ToList());
                
                register.Status = SessionRegisterStatus.Processed;
                Session.Save(register);

                tx.Commit();
            }
        }


        internal void DebitPaidInvoices(Student student, IEnumerable<SessionAttendance> unpaidAttendances )
        {
            var paidInvoices = GetPaidDebitableInvoices(student);
            if (!paidInvoices.Any()) return;

            paidInvoices.ForEach(x => x.DebitInvoiceWithAttendences(unpaidAttendances.ToList()));
            //_studentRepository.Save(student);
            Session.SaveOrUpdate(student);
        }

        internal void CheckForNewInvoiceCreation(Student student, List<SessionAttendance> unpaidAttendances)
        {
            var paidInvoices = GetPaidDebitableInvoices(student);
            if (paidInvoices.Count > 0 || !unpaidAttendances.Any()) return;

            student.Invoices.Add(CreateNewInvoice(student));
            //_studentRepository.Save(student);
            Session.SaveOrUpdate(student);
        }
      
        private Invoice CreateNewInvoice(Student student)
        {
            return new Invoice
                       {
                           Student = student,
                           DateOfGeneration = DateTime.UtcNow,
                           PaymentType = InvoicePaymentType.Advanced,
                           Status = InvoiceStatus.Pending,
                           DiscountApplied = student.DiscountAmount,
                           TotalAmount = student.DefaultPaymentPlan.Amount,
                           TotalAfterDiscount = (student.DefaultPaymentPlan.Amount - student.DiscountAmount),
                           NumberOfSessionsPayingFor = student.DefaultPaymentPlan.TotalSessions
                       };
        }


        //public IEnumerable<TuitionCentre> GetAllCentres()
        //{
        //    return _tuitionCentreRepository.FindAll();
        //}

        public StudentSubjectViewModel StudentNewInvoiceDetails(int Id)
        {
            StudentSubjectViewModel viewModel = new StudentSubjectViewModel();
            var student = Session.Query<Student>().Where(x => x.Id == Id).First();
            viewModel.FirstName = student.FirstName + " " + student.LastName;
            viewModel.LastName = student.LastName;
            viewModel.StudentId = student.Id;
            viewModel.StudentNo = student.StudentNo;

            List<Student> listStudents = new List<Student>();
            listStudents.Add(student);

            var paymentPlans =
                _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));

            List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
            //PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
            //defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
            //lstpaymentplan.Add(defaultpaymentplan);

            foreach (var data in paymentPlans)
            {
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
                lstpaymentplan.Add(paymentPlan);
            }
            List<PaymentTypeCA> lstPaymentType = new List<PaymentTypeCA>();
           // PaymentTypeCA defaultPaymenttype = new PaymentTypeCA();
            //defaultPaymenttype.PaymentTypeId = 0; defaultPaymenttype.PaymentTypeDesc = "--Select--";
            //lstPaymentType.Add(defaultPaymenttype);
            lstPaymentType.AddRange(Enum.GetValues(typeof(DAL.Domain.Enumerations.PaymentType))
               .Cast<DAL.Domain.Enumerations.PaymentType>()
               .Select(ts => new PaymentTypeCA
               {
                   PaymentTypeId = ((int)ts),
                   PaymentTypeDesc = ts.ToString()
               }).ToList());

            lstPaymentType = lstPaymentType.Where(x => x.PaymentTypeDesc != "None").ToList();

            viewModel.lstPaymentPlan = lstpaymentplan;
            viewModel.PaymentPlanSelected = lstpaymentplan[0].PaymentPlanId;
            viewModel.lstPaymentType = lstPaymentType;
            viewModel.PaymentTypeSelected = 0;

            SessionSubjectAttendanceModel sa = new SessionSubjectAttendanceModel();
            sa.StudentId = student.Id;
            sa.PaymentRequired =  _paymentRequired(student);
            //sa.PaymentRequired = true;
            viewModel.SessionAttendanceViewModel = sa;
            return viewModel;
        }

        public SessionRegisterViewModel GetRegister(int? sessionId, DateTime? date)
        {
            if(sessionId!=null)
            {
                return GetSessionRegister(sessionId.GetValueOrDefault(), date);
            }

            return new SessionRegisterViewModel();
        }

        public List<StudentSubjectViewModel> GetStudyPlanDetailsByStudentId(List<int> studentIds, int sessionId, DateTime? date)
        {
            var sessionRegister = GetRegister(sessionId, date);
            var students = _studentRepository.QueryList(x => studentIds.Contains(x.Id));
            var session = _sessionRepository.Get(sessionId);
            return GetStudentRegisterDetails(students,session,sessionRegister);
        }


        public SessionRegisterViewModel GetViewOnlyRegister(int sessionId, DateTime date)
        {
            var register = _sessionAttendanceRepository.GetRegister(sessionId, date);
            if (register == null) throw new NullReferenceException(string.Format("Register with sessionId {0} on date {1} does not exist", sessionId, date));

            var studentsOnRegister = register.SessionAttendances.Select(x => x.Student).ToList();

            return new SessionRegisterViewModel
                       {
                           SessionRegisterId = register.Id,
                           RegisterDetailsAreReadOnly = true,
                           RegisterLocationDetailsAreReadOnly = true,
                           SessionId = sessionId,
                           Center = register.Centre.Id,
                           Date = date.ToString("dd/MM/yyyy"),
                           SessionAttendees = GetReadOnlyRegisterDetails(studentsOnRegister, register.Session, register),
                           ExistingRegisters =null

                       };

        }


        private List<StudentSubjectViewModel> GetReadOnlyRegisterDetails(List<Student> students, Session session, SessionRegister sessionRegister)
        {
            var paymentPlans =
                _paymentPlanRepository.QueryList(x => _studentCurriculums(students).Contains(x.Curriculum));
            var studentSubjectList = new List<StudentSubjectViewModel>();
            students.ForEach(x => PopulateReadonlyStudentRegisterDetails(x, studentSubjectList, session, sessionRegister, paymentPlans));

            return studentSubjectList;
        }


        private void PopulateReadonlyStudentRegisterDetails(Student student, ICollection<StudentSubjectViewModel> studentSubjects, Session session, SessionRegister existingRegister, IEnumerable<PaymentPlan> paymentPlans)
        {
            SessionAttendance attendance = null;            
            attendance = existingRegister.SessionAttendances.FirstOrDefault(sa => sa.Student.Id == student.Id);

            var studentPaymentPlans = _studentPaymentPlans(student, paymentPlans.ToList());
            //var unpaidInvoices = GetUnpaidInvoices(student);
            //var paymentRequired = unpaidInvoices != null && unpaidInvoices.Any(x => x.PaymentType == InvoicePaymentType.Advanced);

            var payments = GetPayments(attendance);
            var payedInvoices = payments.Where(x => x.Invoice != null).Select(x => x.Invoice);        
            //Changes added
            //var paymentWasRequired = payedInvoices != null && payedInvoices.Any(x => x.PaymentType == InvoicePaymentType.Advanced);
            var paymentWasRequired = !_creditsAreAvailable(student);

            var paymentViewModels = MapToPaymentViewModels( payedInvoices.ToList(), GetPayments(attendance), student, studentPaymentPlans);

            
                attendance.SubjectsAttended.ToList().ForEach(sua =>
                {
                    var studentViewModel = GetStudentViewModel(student, session, paymentViewModels, string.Empty, paymentWasRequired);
                    studentViewModel.SessionAttendanceViewModel = GetSessionSubjectAttendance(sua, paymentViewModels, string.Empty, paymentWasRequired);
                    studentSubjects.Add(studentViewModel);
                });
            
        }

        private List<StudentSubjectViewModel> GetStudentRegisterDetails(List<Student> students,Session session, SessionRegister sessionRegister)
        {            
            //if there is no register existing return students from template otherwise return students on register

            var paymentPlans =
                _paymentPlanRepository.QueryList(x => _studentCurriculums(students).Contains(x.Curriculum));

            var studentSubjectList = new List<StudentSubjectViewModel>();
            var date = sessionRegister == null ? DateTime.UtcNow.Localize() : sessionRegister.Date;
            var studentNotes = _sessionAttendanceRepository.GetPreviousNotes(students.Select(x => x.Id).ToList(), date);

            students.ForEach(x => PopulateStudent( x, studentSubjectList,session, sessionRegister, paymentPlans, studentNotes));

            return studentSubjectList;
        }

        private SessionRegister GetRegister(int sessionId, DateTime? date)
        {
           
            return date.HasValue?_sessionAttendanceRepository.GetRegister(sessionId,date.GetValueOrDefault()):null;
        }

        private SessionRegisterViewModel GetSessionRegister(int sessionId, DateTime? date)
        {
            var sessionRegister = GetRegister(sessionId, date);

            List<Student> studentsOnRegister = sessionRegister == null
                                          ? GetStudentBySession(sessionId).ToList()
                                          : sessionRegister.SessionAttendances.Select(x => x.Student).ToList();
            var session = _sessionRepository.Get(sessionId);

            return new SessionRegisterViewModel
                       {
                           SessionRegisterId = sessionRegister!=null?sessionRegister.Id:(int?)null,
                           RegisterDetailsAreReadOnly = sessionRegister != null && sessionRegister.Status == SessionRegisterStatus.Processed,
                           RegisterLocationDetailsAreReadOnly = sessionRegister != null,
                           SessionId = sessionId,
                           Center = sessionRegister!=null?sessionRegister.Centre.Id:(int?)null,
                           Date = date.HasValue?date.Value.ToString("dd/MM/yyyy"):null,
                           SessionAttendees = GetStudentRegisterDetails(studentsOnRegister,session, sessionRegister),
                           ExistingRegisters =
                               _sessionRegisterRepository.QueryList(
                                   x => x.Session.Id == sessionId && x.Date > _dateToSearchExistingRegistersFrom).
                               Select(x => x.Date).ToList()
                       };
        }
        


        public int SaveSessionAttendances(SessionRegisterViewModel viewModel, int userId)
        {

            DateTime sessionDate;

            ValidateRegister(viewModel, out sessionDate);
            var classSessionData = PopulateSessionData(viewModel, sessionDate);         
            //had to expose the Nhibernate Session and create a transation as hosting provider did not support MSDTC
            using (var tx = Session.BeginTransaction(IsolationLevel.Serializable))
            {
                                
                SaveAttendances(classSessionData.Students, classSessionData.SessionSubjectAttendances,
                                classSessionData.SessionRegister, classSessionData.Teachers,
                                classSessionData.ClassSession, sessionDate);

                tx.Commit();
                return classSessionData.SessionRegister.Id;
            }
        }

        private ClassSessionData PopulateSessionData(SessionRegisterViewModel viewModel, DateTime sessionDate)
        {
            var centre =  _tuitionCentreRepository.Get(viewModel.Center.GetValueOrDefault());
            var studentsIdsFromUi = viewModel.SessionAttendees.Select(s => s.StudentId).ToList();
            var classSession = _sessionRepository.Get(viewModel.SessionId);

            return new ClassSessionData()
                       {
                           ClassSession = classSession,
                           SessionSubjectAttendances =
                               viewModel.SessionAttendees.Select(x => x.SessionAttendanceViewModel).ToList(),
                           Students = _studentRepository.QueryList(x => studentsIdsFromUi.Contains(x.Id)),
                           Teachers = _teacherRepository.FindAll().ToList(),
                           SessionRegister =
                               GetSessionRegister(viewModel.SessionRegisterId, centre, sessionDate, classSession)
                       };
        }


        private void ValidateRegister(SessionRegisterViewModel viewModel, out DateTime sessionDate)
        {
            if (string.IsNullOrEmpty(viewModel.Date) || !DateTime.TryParse(viewModel.Date, out sessionDate))
                throw new Exception("A valid Date must be provided");

            if (viewModel.SessionId == 0)
                throw new Exception("A Session must be selected");

            if (SessionAlreadyExists(viewModel.SessionRegisterId, viewModel.SessionId, sessionDate))
                throw new Exception("A register for this date already exists");           

        }

        //Should only be called within a transaction
        private void SaveAttendances(List<Student> students, List<SessionSubjectAttendanceModel> sessionSubjectAttendances,
            SessionRegister sessionRegister, List<Teacher> teachers, Session session, DateTime sessionDate )
        {
            //remove any deleted attendances
            RemoveDeletedAttendances(sessionSubjectAttendances, sessionRegister);
            Session.Flush();

            students.ForEach(student =>
            {
                var subjectAttendances =
                    sessionSubjectAttendances.Where(x => x.StudentId == student.Id);
                if (!subjectAttendances.Any()) return;
                AddOrUpdateSessionAttendance(subjectAttendances.ToList(),
                                             sessionRegister,
                                             student,
                                             teachers,
                                             session,
                                             sessionDate);
                Session.Save(sessionRegister);
            });

        }

        private static void RemoveDeletedAttendances(IEnumerable<SessionSubjectAttendanceModel> sessionSubjectAttendances, SessionRegister sessionRegister )
        {
            //get attendances where its is already saved previously and has an Id
            var savedAttenances = sessionSubjectAttendances.Where(sa => sa.AttendanceId != null).ToList();
            if (!savedAttenances.Any()) return;
            var deletedAttendances =
                sessionRegister.SessionAttendances.Where(
                    x => (!savedAttenances.Select(p => p.AttendanceId).Contains(x.Id))).ToList();
            deletedAttendances.ForEach(sa => sessionRegister.SessionAttendances.Remove(sa));
        }


        private SessionRegister GetSessionRegister(int? registerId, TuitionCentre centre, DateTime sessionDate, Session session)
        {
            return registerId != 0 && registerId != null
                       ? new SessionAttendanceRepository().GetRegister(registerId.GetValueOrDefault())
                       : CreateNewSessionRegister(session, sessionDate, centre);
        }

        private bool SessionAlreadyExists(int? registerId, int sessionId, DateTime sessionDate)
        {
            return (registerId == null || registerId < 1) && _sessionRegisterRepository.
                                                                 QueryList(
                                                                     x =>
                                                                     x.Date == sessionDate && x.Session.Id == sessionId)
                                                                 .FirstOrDefault() != null;
        }


        public RegisterSearchResultDto SearchRegisters(SearchRegisterDto searchRegister)
        {
            return new SessionAttendanceRepository().SearchForSessionRegisters(searchRegister);
        }

        private static SessionRegister CreateNewSessionRegister(Session session, DateTime date, TuitionCentre centre)
        {
            return new SessionRegister
                       {
                           Session = session,
                            Date = date,
                            Status = SessionRegisterStatus.Pending,
                            Centre = centre

                       };
        }


        private void AddOrUpdateSessionAttendance(List<SessionSubjectAttendanceModel> subjects,
            SessionRegister sessionRegister, Student student, List<Teacher> teachers, Session session, DateTime date)
        {
            var sessionAttendance = _getExistingAttendanceForStudent(student, subjects);
            var subjectModel = subjects.First();

            if (sessionAttendance != null)
            {
                sessionAttendance.Status = subjectModel.Status;
                sessionAttendance.SessionRegister = sessionRegister;
                sessionAttendance.SubjectsAttended.Clear();
                //Changes added -- stop clearing the payments
                sessionAttendance.Payments.Clear();
                //_sessionAttendanceRepository.Save(sessionAttendance);          
                Session.Save(sessionAttendance);
                Session.Flush();
            }
            else
            {
                sessionAttendance = SessionAttendance.CreateAttendance(session, date,
                    student, sessionRegister, subjectModel);
                sessionRegister.SessionAttendances.Add(sessionAttendance);
            }

            AddSubjectsToAttendance(subjects, sessionAttendance, session.SubjectsTaughtAtSession.ToList(), teachers);

            AddPaymentsToAttendance(subjectModel, sessionAttendance);
        }

        //Changes added
        private SessionAttendance GetOutstandingAttendance(Student student, SessionAttendance sessionAttendance)
        {
            var outstandingInvoice = student.Invoices?.FirstOrDefault(x => x.Status == InvoiceStatus.Pending);
            SessionAttendance outstandingAttendance = null;
            if (outstandingInvoice != null)
                outstandingAttendance =
                    student.SessionAttendances?.FirstOrDefault(x =>
                        x.Invoice == outstandingInvoice && x != sessionAttendance);
            return outstandingAttendance;
        }

        private void AddSubjectsToAttendance(List<SessionSubjectAttendanceModel> subjectAttendance, SessionAttendance sessionAttendance, IEnumerable<Subject> subjects, IEnumerable<Teacher> teachers)
        {
            subjectAttendance.ForEach(x => sessionAttendance.SubjectsAttended.Add(
                CreateSubjectAttendance(subjects.First(s => s.Id == x.SubjectId), sessionAttendance,
                                        teachers.FirstOrDefault(t => t.Id == x.TeacherId), x.Notes)
                                               ));
        }


        private void AddPaymentToAttendance(PaymentViewModel payment, SessionAttendance sessionAttendance)
        {
            var invoice = Session.Get<Invoice>(payment.InvoiceId); //_invoiceRepository.Get(payment.InvoiceId);
            var paymentPlan = payment.PaymentAmount.HasValue ? Session.Get<PaymentPlan>(payment.PaymentAmount.GetValueOrDefault()) : null; //_paymentPlanRepository.Get(payment.PaymentAmount.GetValueOrDefault()) : null;


            sessionAttendance.Payments.Add(
                new InvoicePayment
                {
                    Attendance = sessionAttendance,
                    ChequeNo = payment.ChequeNo,
                    PaymentType = payment.PaymentType.GetValueOrDefault(),
                    PaymentAmountId = paymentPlan != null ? paymentPlan.Id : (int?) null,
                    //if discounted payment then show discounted payment otherwise show payment
                    PaymentAmount =
                        paymentPlan != null
                            ? sessionAttendance.Student.GetPaymentAmount(paymentPlan, invoice)//paymentPlan.Amount
                            : (decimal?) null,  
                    NumberOfSessionsPaidFor = paymentPlan != null ? paymentPlan.TotalSessions : 0,
                    PaymentDate = !string.IsNullOrEmpty(payment.PaymentDate)
                        ? DateTime.Parse(payment.PaymentDate)
                        : (DateTime?) null,
                    Invoice = invoice
                });
        }
        

        //Changes Added
        private void UpdateExistingPayment(PaymentViewModel payment, InvoicePayment existingPayment, PaymentPlan paymentPlan,
            Invoice invoice)
        {
            existingPayment.ChequeNo = payment.ChequeNo;
            existingPayment.PaymentType = payment.PaymentType.GetValueOrDefault();
            existingPayment.PaymentAmountId = paymentPlan != null ? paymentPlan.Id : (int?) null;
            existingPayment.PaymentAmount = paymentPlan != null ? paymentPlan.Amount : (decimal?) null;
            existingPayment.NumberOfSessionsPaidFor = paymentPlan != null ? paymentPlan.TotalSessions : 0;
            existingPayment.PaymentDate = !string.IsNullOrEmpty(payment.PaymentDate)
                ? DateTime.Parse(payment.PaymentDate)
                : (DateTime?) null;
            existingPayment.Invoice = invoice;
            Session.SaveOrUpdate(existingPayment);
        }
        private void AddPaymentsToAttendance(SessionSubjectAttendanceModel subjectAttendanceModel, SessionAttendance sessionAttendance)
        {
            if (subjectAttendanceModel.FutureInvoicePayment != null && subjectAttendanceModel.FutureInvoicePayment.PaymentType != null)
            {
                var payment = subjectAttendanceModel.FutureInvoicePayment;

                AddPaymentToAttendance(payment, sessionAttendance);
            }
            if (subjectAttendanceModel.OutstandingPayments != null)
                subjectAttendanceModel.OutstandingPayments.Where(x => x.PaymentType != null).ToList().
                    ForEach(x => AddPaymentToAttendance(x, sessionAttendance));
        }
       

        private SubjectAttendance CreateSubjectAttendance(Subject subject, SessionAttendance sessionAttendance,Teacher teacher, string notes)
        {
            return new SubjectAttendance
                       {
                           Subject = subject,
                           Attendance = sessionAttendance,
                           Teacher = teacher,
                           Notes = notes
                       };
        }

        private IEnumerable<Student> GetStudentBySession(int sessionId)
        {
            return _studentRepository.QueryList(x => x.Subjects.Any(s => s.Session.Id == sessionId) && x.Enabled);
        }


        private static IEnumerable<InvoicePayment> GetPayments(SessionAttendance attendance)
        {
            return attendance == null ? null : attendance.Payments;
        }

        private void PopulateStudent(Student student,ICollection<StudentSubjectViewModel> studentSubjects, Session session, SessionRegister existingRegister, IEnumerable<PaymentPlan> paymentPlans, List<StudentNoteDto> notes   )
        {
            SessionAttendance attendance = null;
            if(existingRegister!=null)
            attendance = existingRegister.SessionAttendances.FirstOrDefault(sa => sa.Student.Id == student.Id);

            var studentPaymentPlans = _studentPaymentPlans(student, paymentPlans.ToList());
            var unpaidInvoices = GetUnpaidInvoices(student);
            //Changes added 
            //var paymentRequired = unpaidInvoices != null && unpaidInvoices.Any(x => x.PaymentType == InvoicePaymentType.Advanced);
            var paymentRequired = _paymentRequired(student);
            var previousNotes = _getPreviousNotes(notes, student);

            var paymentViewModels = MapToPaymentViewModels(unpaidInvoices, GetPayments(attendance), student, studentPaymentPlans);

            if (attendance == null)
            {                
                var studentViewModel = GetStudentViewModel(student, session, paymentViewModels,previousNotes, paymentRequired);

                studentSubjects.Add(studentViewModel);
            }
            else
            {
                attendance.SubjectsAttended.ToList().ForEach(sua =>
                                                                 {
                                                                     var studentViewModel = GetStudentViewModel(student, session, paymentViewModels,previousNotes, paymentRequired);
                                                                     studentViewModel.SessionAttendanceViewModel = GetSessionSubjectAttendance(sua, paymentViewModels,previousNotes, paymentRequired);
                                                                     studentSubjects.Add(studentViewModel);
                                                                 });
            }
        }

        


        private IEnumerable<DiscountedPaymentAmount> AdjustPaymentPlanAfterDiscount(Student student, List<PaymentPlan> paymentPlans)
        {
            var paymentPlansAfterDiscount = new List<DiscountedPaymentAmount>();
            paymentPlans.ForEach(x =>
                                     {
                                         var discountedAmount = Mapper.Map<DiscountedPaymentAmount>(x);
                                         if (x.Id == student.DefaultPaymentPlan.Id && student.DiscountAmount != 0)
                                         {
                                             discountedAmount.PaymentAmountInCurrency = student.DiscountedPaymentString();
                                             discountedAmount.PaymentAmount = student.DiscountedPayment();
                                         }
                                         paymentPlansAfterDiscount.Add(discountedAmount);
                                     });

            //add this just incase student want to defer payment
            //paymentPlansAfterDiscount.Add(new DiscountedPaymentAmount{Id=-1, PaymentAmountInCurrency = "Defer Payment"});

            return paymentPlansAfterDiscount;
        }

        private StudentSubjectViewModel GetStudentViewModel(Student student, Session session, List<PaymentViewModel> payments,string previousNotes, bool paymentRequired)
        {
            return new StudentSubjectViewModel
                       {
                           Subjects = student.Subjects.Distinct(new StudentSubjectComparer()).
                           Select(x => new SubjectViewModel
                           {
                               SubjectId = x.Subject.Id,
                               Name = x.Subject.Name,
                               Level = x.Subject.Level
                           }).ToList(),
                           StudentId = student.Id,
                           FirstName = student.FirstName,
                           LastName = student.LastName,
                           StudentNo = student.StudentNo,
                           SessionAttendanceViewModel =
                               new SessionSubjectAttendanceModel
                                   {                                       
                                       SubjectId = _getSubjectStudiedAtSessionId(student, session),
                                       PaymentRequired = paymentRequired,
                                       StudentId = student.Id,
                                       //Changes Added
                                   OutstandingPayments = payments.Where(x=> GetOutstandingDailyInvoices(student).Any(y=>y.Id== x.InvoiceId)).ToList(), //payments.Where(x=> x.IsFutureInvoice == false).ToList(),
                                   FutureInvoicePayment = payments.FirstOrDefault(x=> x.IsFutureInvoice),
                                       PreviousNotes = previousNotes
                                   }
                       };
        }

        private List<PaymentViewModel> MapToPaymentViewModels(List<Invoice> invoices, IEnumerable<InvoicePayment> payments, Student student, List<PaymentPlan> paymentPlans)
        {
            var paymentViewModelList = new List<PaymentViewModel>();
            GenerateInvoicePaymentViewModels(invoices, payments, student, paymentPlans, paymentViewModelList);

            //Changes added -- this will be refactored 
            var futureInvoicePayment = payments != null ? payments.FirstOrDefault(i => i.Invoice == null) : null;



            //for future invoices generate payment view model based on the students current payment plan
            var futurePaymentModel = new PaymentViewModel
            {
                IsFutureInvoice = true,
                PaymentDate = futureInvoicePayment != null && futureInvoicePayment.PaymentDate.HasValue ?
        futureInvoicePayment.PaymentDate.Value.ToString("dd/MM/yyyy") : null,
                PaymentAmount = futureInvoicePayment != null
                ? futureInvoicePayment.PaymentAmountId
                : (int?)null,
                ChequeNo = futureInvoicePayment != null ? futureInvoicePayment.ChequeNo : null,
                PaymentType = futureInvoicePayment != null
                ? futureInvoicePayment.PaymentType
                : (PaymentType?)null,
                PaymentPlans = FilterPaymentPlanByInvoiceType(null, paymentPlans, student)
            };

            paymentViewModelList.Add(futurePaymentModel);

            return paymentViewModelList;
        }

        private void GenerateInvoicePaymentViewModels(List<Invoice> invoices, IEnumerable<InvoicePayment> payments, Student student, List<PaymentPlan> paymentPlans,
            List<PaymentViewModel> paymentViewModelList)
        {
            //daily invoices only
            invoices.ForEach(x =>
            {
                //Changes added
                //var existingPayment = payments != null ? payments.FirstOrDefault(i => i.Invoice.Id == x.Id) : null;
                var existingPayment = payments != null
                    ? payments.FirstOrDefault(i => i.Invoice != null &&
                                                   invoices.OrderByDescending(y => y.DateOfGeneration).FirstOrDefault(b =>
                                                       b.Id == x.Id
                                                       && i.Invoice.Id == x.Id) != null
                    )
                    : null;
                //b=> b.Id == x.Id
                var model = new PaymentViewModel
                {
                    InvoiceId = x.Id,
                    IsFutureInvoice = x.PaymentType == InvoicePaymentType.Advanced,
                    PaymentDate = existingPayment != null && existingPayment.PaymentDate.HasValue
                        ? existingPayment.PaymentDate.Value.ToString("dd/MM/yyyy")
                        : null,
                    PaymentAmount = existingPayment != null
                        ? existingPayment.PaymentAmountId
                        : (int?) null,
                    ChequeNo = existingPayment != null ? existingPayment.ChequeNo : null,
                    PaymentType = existingPayment != null
                        ? existingPayment.PaymentType
                        //Changes added -- set default Payment Type as None when Invoice is outstanding daily invoice
                        //: (x.Status==InvoiceStatus.Pending? PaymentType.None : (PaymentType?)null),
                        : (PaymentType?) null,
                    PaymentPlans = FilterPaymentPlanByInvoiceType(x, paymentPlans, student)
                };
                paymentViewModelList.Add(model);
            });
        }

        private List<DiscountedPaymentAmount> FilterPaymentPlanByInvoiceType(Invoice invoice, List<PaymentPlan> paymentPlans, Student student)
        {
            //Changes added
            var dailyRate = paymentPlans.FirstOrDefault(x => x.TotalSessions == 1);
            return invoice == null
                       ? AdjustPaymentPlanAfterDiscount(student, paymentPlans).ToList()
                       : new List<DiscountedPaymentAmount> { Mapper.Map<DiscountedPaymentAmount>(dailyRate) };
        }

        private SessionSubjectAttendanceModel GetSessionSubjectAttendance(SubjectAttendance subjectAttendance, List<PaymentViewModel> payments, string previousNotes, bool paymentRequired)
        {
            var outstandingInvoices = Session.Query<Invoice>().ToList()
                .Where(x => x.PaymentType == InvoicePaymentType.Daily && x.PaymentReciept == null)
                .OrderByDescending(x => x.DateOfGeneration);
            return new SessionSubjectAttendanceModel
                       {
                           StudentId = subjectAttendance.Attendance.Student.Id,
                           AttendanceId = subjectAttendance.Attendance.Id,
                           TeacherId = subjectAttendance.Teacher!=null? subjectAttendance.Teacher.Id:(int?)null,
                           Status = subjectAttendance.Attendance.Status,
                           SubjectId = subjectAttendance.Subject.Id,
                           PaymentRequired = paymentRequired,
                           //Changes added
                           OutstandingPayments = payments.Where(x=> outstandingInvoices.Any(y=>y.Id==x.InvoiceId)).ToList(),//payments.Where(x => !x.IsFutureInvoice).ToList(),
                           FutureInvoicePayment = payments.FirstOrDefault(x => x.IsFutureInvoice),
                           Notes = subjectAttendance.Notes,
                           PreviousNotes = previousNotes
                       };
        }
    }

    public class ClassSessionData
    {
        public List<SessionSubjectAttendanceModel> SessionSubjectAttendances { get; set; }

        public List<Student> Students { get; set; }

        public List<Teacher> Teachers { get; set; }

        public Session ClassSession { get; set; }

        public SessionRegister SessionRegister { get; set; }
    }
}
