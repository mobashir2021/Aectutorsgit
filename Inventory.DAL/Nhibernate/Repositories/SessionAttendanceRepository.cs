using System;
using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AutoMapper;
using NHibernate;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public class SessionAttendanceRepository : Repository<SessionAttendance, int>
    {
        private readonly ISession _session;
        private readonly IRepository<PaymentPlan, int> _paymentPlanRepository;
        private readonly Func<List<Student>, List<Curriculum>> _studentCurriculums =
            list => list.Select(x => x.Curriculum).Distinct().ToList();
        public SessionAttendanceRepository() : this(null)
        {
            _paymentPlanRepository = new Repository<PaymentPlan, int>();
        }

        public SessionAttendanceRepository(ISession session)
        {
            _session = session ?? SessionManager.GetSession<SessionAttendance>();
            _paymentPlanRepository = new Repository<PaymentPlan, int>();
        }

        protected override ISession Session
        {
            get { return _session; }
        }


        public SessionRegister GetRegister(int sessionId, DateTime date)
        {
            //return (from sr in Session.Query<SessionRegister>()
            //        join sa in Session.Query<SessionAttendance>() on sr.Id equals sa.SessionRegister.Id
            //        join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
            //        from p in sa.Payments.DefaultIfEmpty()                    
            //        where sr.Session.Id == sessionId && sr.Date == date
            //        select sr).FirstOrDefault();

            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            return Session.QueryOver<SessionRegister>().
                JoinAlias(x => x.SessionAttendances, () => sa).
                JoinAlias(x => sa.SubjectsAttended, () => ssa).
                Left.JoinAlias(x => sa.Payments, () => pa).
                Where(x => x.Session.Id == sessionId && x.Date == date).List().FirstOrDefault();
        }

        public SessionRegister GetRegister(int id)
        {
            //return (from sr in Session.Query<SessionRegister>()
            //        join sa in Session.Query<SessionAttendance>() on sr.Id equals sa.SessionRegister.Id
            //        join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
            //        join ip in Session.Query<InvoicePayment>() on sa.Id equals ip.Attendance.Id
            //        where sr.Id == id
            //        select sr).FirstOrDefault();   
            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            return Session.QueryOver<SessionRegister>().
                JoinAlias(x => x.SessionAttendances, () => sa).
                JoinAlias(x => sa.SubjectsAttended, () => ssa).
                Left.JoinAlias(x => sa.Payments, () => pa).
                Where(x => x.Id == id).List().FirstOrDefault();
        }

        public Invoice CalculateVATAndSequenceNumber(Invoice oldinvoice)
        {
            var InvoiceSequencing = Session.Query<InvoiceSequencing>().Where(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate).First();
            var objVATDetails = Session.Query<VATDetails>().Where(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate).First();
            var invoices = Session.Query<Invoice>().Where(x => x.DateOfGeneration <= objVATDetails.ToDate && x.DateOfGeneration >= objVATDetails.FromDate).ToList();
            if (oldinvoice.Id > 0)
            {
                int count = invoices.Where(x => x.Id < oldinvoice.Id).Count();
                oldinvoice.InvoiceRefNumber = InvoiceSequencing.SequenceStartNum + count + 1;
                oldinvoice.VATAmount = Math.Round(((objVATDetails.VATPercentage * oldinvoice.TotalAfterDiscount) / 100) / Convert.ToDecimal(1.2), 2);
                oldinvoice.TotalExcludeVAT = oldinvoice.TotalAfterDiscount - oldinvoice.VATAmount;
            }
            else
            {
                int count = invoices.Count();
                oldinvoice.InvoiceRefNumber = InvoiceSequencing.SequenceStartNum + count + 1;
                oldinvoice.VATAmount = Math.Round(((objVATDetails.VATPercentage * oldinvoice.TotalAfterDiscount) / 100) / Convert.ToDecimal(1.2), 2);
                oldinvoice.TotalExcludeVAT = oldinvoice.TotalAfterDiscount - oldinvoice.VATAmount;
            }
            return oldinvoice;
        }


        public List<StudentNoteDto> GetPreviousNotes(List<int> studentIds, DateTime currentSessionDate)
        {
            var date = currentSessionDate.AddMonths(-2);
            //&& sr.Date!= currentSessionDate         
            return (from sr in Session.Query<SessionRegister>()
                    join sa in Session.Query<SessionAttendance>() on sr.Id equals sa.SessionRegister.Id
                    join sas in Session.Query<SubjectAttendance>() on sa.Id equals sas.Attendance.Id
                    where studentIds.Contains(sa.Student.Id) && sr.Date > date && sr.Status == Domain.Enumerations.SessionRegisterStatus.Processed && sas.Notes != null
                    select new StudentNoteDto { Date = sr.Date, Note = sas.Notes, StudentId = sa.Student.Id }).ToList();
            //p.GroupBy(x=> x.Id).Select()

        }

        public StudentAttendanceDetails GetStudentAttendance(int SessionAttendanceId)
        {
            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            Student s = null;
            Session session = null;
            SessionRegister r = null;
            Teacher t = null;
            var query = Session.QueryOver<SessionAttendance>().
                JoinAlias(x => x.SubjectsAttended, () => ssa).
                JoinAlias(x => x.Student, () => s).
                JoinAlias(x => x.SessionRegister, () => r).
                JoinAlias(x => r.Session, () => session).
                JoinAlias(x => ssa.Teacher, () => t).
                Left.JoinAlias(x => x.Payments, () => pa).Where(x => x.Id == SessionAttendanceId);
            var Payments = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == SessionAttendanceId && x.Invoice != null
                && x.Invoice.Status != InvoiceStatus.Cancelled).ToList();
            SessionAttendance sessionAttendance = query.List().FirstOrDefault();
            sessionAttendance.Payments = Payments;
            StudentAttendanceDetails studentAttendanceDetails = new StudentAttendanceDetails();
            studentAttendanceDetails.Date = sessionAttendance.SessionRegister.Date.ToString("yyyy-MM-dd");
            studentAttendanceDetails.InvoiceId = sessionAttendance.Invoice != null ? sessionAttendance.Invoice.Id : 0;
            //Changes added -- Removed column
            //RemainingCredits = x.RemainingCredits,
            studentAttendanceDetails.SessionAttendanceId = sessionAttendance.Id;
            studentAttendanceDetails.Status = sessionAttendance.Status.ToString();


            List<Student> listStudents = new List<Student>();
            listStudents.Add(sessionAttendance.Student);
            List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
            PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
            defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
            lstpaymentplan.Add(defaultpaymentplan);
            //GetPaymentPlans(Payments, listStudents, ref lstpaymentplan);


            Curriculum curriculum = sessionAttendance.Student.Curriculum;
            
            var allPaymentPlans = this._session.Query<PaymentPlan>().ToList();
            List<PaymentPlan> paymentPlans = new List<PaymentPlan>();
            foreach(var paymentcurriculum in allPaymentPlans)
            {
                if((int)paymentcurriculum.Curriculum == (int)curriculum)
                {
                    paymentPlans.Add(paymentcurriculum);
                }
            }
            
            



            foreach (var data in paymentPlans)
            {
                if (data.TotalSessions > 1)
                    continue;
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
                lstpaymentplan.Add(paymentPlan);
            }
            List<PaymentTypeCA> lstPaymentType = GetPaymentTypes();

            studentAttendanceDetails.lstPaymentPlan = lstpaymentplan;
            studentAttendanceDetails.PaymentPlanSelected = 0;
            studentAttendanceDetails.lstPaymentType = lstPaymentType;
            studentAttendanceDetails.PaymentTypeSelected = 0;
            if (sessionAttendance.Invoice != null)
            {

                List<FutureInvoiceAvailable> lstFuture = new List<FutureInvoiceAvailable>();
                FutureInvoiceAvailable fa = new FutureInvoiceAvailable();
                fa.FIAId = 0; fa.FIAValue = "Choose Future Invoice";
                studentAttendanceDetails.listFutureInvoices = new List<FutureInvoiceAvailable>();
                studentAttendanceDetails.listFutureInvoices.Add(fa);

                var invoiceRef = Session.Query<Invoice>()?.FirstOrDefault(x => x.Id == sessionAttendance.Invoice.Id);
                var invoicePaymentRef =
                    Session.Query<PaymentReciept>()?.FirstOrDefault(x => x.Invoice.Id == invoiceRef.Id);
                GetAttendanceCreditBasedOnInvoicePayment(SessionAttendanceId, sessionAttendance, studentAttendanceDetails, invoicePaymentRef);
                GetFutureInvoiceAvailable(sessionAttendance, studentAttendanceDetails);
            }
            else
            {
                var paymentInvoice = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == SessionAttendanceId).FirstOrDefault();
                if (paymentInvoice != null)
                {
                    if (paymentInvoice.NumberOfSessionsPaidFor > 1)
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                        studentAttendanceDetails.RemainingCredits = paymentInvoice.NumberOfSessionsPaidFor;
                        string idvalueinv = paymentInvoice.Invoice != null ? paymentInvoice.Invoice.Id.ToString() : "";
                        studentAttendanceDetails.ChargedTo = "Invoice " + idvalueinv;
                    }
                    else
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "true";
                        studentAttendanceDetails.RemainingCredits = 0;
                    }
                }
                else
                {
                    var sessionAttended = Session.Query<SessionAttendance>().Where(x => x.Id == SessionAttendanceId).First();
                    var student = Session.Query<Student>().Where(x => x.Id == sessionAttended.Student.Id).First();
                    var invoices = Session.Query<Invoice>().Where(x => x.Student.Id == student.Id).ToList();
                    bool isMatchFound = false;
                    isMatchFound = GetAttendanceCreditUsingInvoiceIfPossible(studentAttendanceDetails, invoices, isMatchFound);
                    if (!isMatchFound)
                    {
                        isMatchFound = GetAttendanceCreditUsingDirectSearchByStudent(studentAttendanceDetails, student, isMatchFound);
                    }
                    if (!isMatchFound)
                    {
                        studentAttendanceDetails.RemainingCredits = 0;
                        studentAttendanceDetails.IsDailyInvoiceData = "true";
                        isMatchFound = true;
                    }
                }
            }


            studentAttendanceDetails.Session = "Session On " + sessionAttendance.SessionRegister.Session.Day.ToString() + " at "
                                            + sessionAttendance.SessionRegister.Session.From.ToString() + "-" + sessionAttendance.SessionRegister.Session.To.ToString();
            studentAttendanceDetails.StudentName =
                string.Format("{0} {1}", sessionAttendance.Student.FirstName, sessionAttendance.Student.LastName);
            studentAttendanceDetails.StudentNo = sessionAttendance.Student.StudentNo;
            studentAttendanceDetails.Teacher =
                string.Format("{0}  {1}",
                              sessionAttendance.SubjectsAttended.FirstOrDefault().Teacher.FirstName,
                              sessionAttendance.SubjectsAttended.FirstOrDefault().Teacher.LastName);
            studentAttendanceDetails.PaymentDetails = sessionAttendance.Payments.Select(d => new StudentPaymentDetails()
            {
                InvoiceNo =
                                                               d.Invoice == null ? "0" : d.Invoice.Id.ToString(),
                InvoiceType =
                                                               d.Invoice == null ? "" : d.Invoice.PaymentType.
                                                                ToString(),
                PaymentAmount =
                                                               d.PaymentAmount.HasValue ? d.PaymentAmount.Value.ToString("C") : null,
                PaymentType =
                                                                d.PaymentType.ToString()
            }).ToList();
            return studentAttendanceDetails;
        }

        private bool GetAttendanceCreditUsingDirectSearchByStudent(StudentAttendanceDetails studentAttendanceDetails, Student student, bool isMatchFound)
        {
            var sessionAttendancesList = Session.Query<SessionAttendance>().Where(x => x.Student.Id == student.Id).ToList();
            foreach (var saL in sessionAttendancesList)
            {
                var attCredit = Session.Query<AttendanceCredit>().Where(x => x.Attendance.Id == saL.Id).FirstOrDefault();
                if (attCredit != null)
                {
                    var invReceipt = Session.Query<PaymentReciept>().Where(x => x.Id == attCredit.Receipt.Id).First();
                    var totalcredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invReceipt.Id).Count();
                    var tempSA = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invReceipt.Id && x.Attendance == null).ToList();
                    if (tempSA.Count > 0)
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                        studentAttendanceDetails.RemainingCredits = tempSA.Count;
                        string invidvalue = invReceipt.Invoice != null ? invReceipt.Invoice.Id.ToString() : "";
                        if (studentAttendanceDetails.RemainingCredits == 1)
                            studentAttendanceDetails.ChargedTo = "Invoice " + invidvalue;
                        else
                            studentAttendanceDetails.ChargedTo = "Credit " + ((totalcredit - tempSA.Count) + 1).ToString() + " of Invoice " + invidvalue;
                        isMatchFound = true;
                        break;
                    }
                }
            }

            return isMatchFound;
        }

        private bool GetAttendanceCreditUsingInvoiceIfPossible(StudentAttendanceDetails studentAttendanceDetails, List<Invoice> invoices, bool isMatchFound)
        {
            foreach (var inv in invoices)
            {
                var invoicereceipt = Session.Query<PaymentReciept>().Where(x => x.Invoice.Id == inv.Id).FirstOrDefault();
                if (invoicereceipt != null)
                {
                    var attCredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invoicereceipt.Id && x.Attendance == null).ToList();
                    if (attCredit.Count > 0)
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                        studentAttendanceDetails.RemainingCredits = attCredit.Count;
                        if (studentAttendanceDetails.RemainingCredits == 1)
                            studentAttendanceDetails.ChargedTo = "Invoice " + inv.Id;
                        else
                            studentAttendanceDetails.ChargedTo = "Credit " + ((inv.NumberOfSessionsPayingFor - attCredit.Count) + 1).ToString() + " of Invoice " + inv.Id.ToString();
                        isMatchFound = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }

                }
            }

            return isMatchFound;
        }

        private void GetFutureInvoiceAvailable(SessionAttendance sessionAttendance, StudentAttendanceDetails studentAttendanceDetails)
        {
            var temp = Session.Query<Invoice>().Where(x => x.Student.Id == sessionAttendance.Student.Id && studentAttendanceDetails.InvoiceId != x.Id && x.NumberOfSessionsPayingFor > 1 && x.Status != InvoiceStatus.Cancelled && x.PaymentReciept != null).ToList();
            foreach (var tempinvoice in temp)
            {
                if (tempinvoice.PaymentReciept == null)
                    continue;
                //if (count == 0)
                //    continue;
                var attendancecredit = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == tempinvoice.PaymentReciept.Id && x.Attendance != null).List().ToList();
                if (attendancecredit.Count == tempinvoice.NumberOfSessionsPayingFor)
                    continue;
                int cnt = attendancecredit.Count + 1;
                FutureInvoiceAvailable futureInvoice = new FutureInvoiceAvailable();
                futureInvoice.FIAId = tempinvoice.Id; futureInvoice.FIAValue = "Credit " + cnt.ToString() + " (Invoice " + tempinvoice.Id.ToString() + ")";
                if (attendancecredit.Count + 1 == tempinvoice.NumberOfSessionsPayingFor)
                    studentAttendanceDetails.ChargedTo = "Invoice " + tempinvoice.Id.ToString();
                else
                    studentAttendanceDetails.ChargedTo = futureInvoice.FIAValue;
                //lstFuture.Add(futureInvoice);
                studentAttendanceDetails.listFutureInvoices.Add(futureInvoice);
                studentAttendanceDetails.IsFutureInvoiceAvailable = true;

            }
            studentAttendanceDetails.FutureInvoiceSelect = 0;
        }

        private void GetAttendanceCreditBasedOnInvoicePayment(int SessionAttendanceId, SessionAttendance sessionAttendance, StudentAttendanceDetails studentAttendanceDetails, PaymentReciept invoicePaymentRef)
        {
            if (invoicePaymentRef == null)
            {
                var paymentInvoice = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == SessionAttendanceId).First();
                if (paymentInvoice.NumberOfSessionsPaidFor > 1)
                {
                    studentAttendanceDetails.IsDailyInvoiceData = "false";
                    studentAttendanceDetails.RemainingCredits = paymentInvoice.NumberOfSessionsPaidFor;
                    studentAttendanceDetails.ChargedTo = "Invoice " + sessionAttendance.Invoice.Id.ToString();
                }
                else
                {
                    studentAttendanceDetails.IsDailyInvoiceData = "true";
                    studentAttendanceDetails.RemainingCredits = 0;
                }
            }
            else
            {
                var count = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == invoicePaymentRef.Id)
                    .List().Count();
                if (count > 0)
                {
                    studentAttendanceDetails.IsDailyInvoiceData = "false";
                    var result = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == invoicePaymentRef.Id)
                        .List().First().Receipt.Id;
                    studentAttendanceDetails.RemainingCredits = Session.QueryOver<AttendanceCredit>()
                        .Where(x => x.Receipt.Id == result && x.Attendance == null).List().Count();
                    //if ((count - studentAttendanceDetails.RemainingCredits) + 1 == count)
                    if (studentAttendanceDetails.RemainingCredits > 0)
                        studentAttendanceDetails.ChargedTo =
                            "Credit " + ((count - studentAttendanceDetails.RemainingCredits) + 1).ToString() +
                            " of Invoice " + sessionAttendance.Invoice.Id.ToString();
                    else
                    {
                        studentAttendanceDetails.ChargedTo = "New Invoice";
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                    }
                }
                else
                    studentAttendanceDetails.IsDailyInvoiceData = "true";
            }
        }

        private static List<PaymentTypeCA> GetPaymentTypes()
        {
            List<PaymentTypeCA> lstPaymentType = new List<PaymentTypeCA>();
            PaymentTypeCA defaultPaymenttype = new PaymentTypeCA();
            defaultPaymenttype.PaymentTypeId = 0; defaultPaymenttype.PaymentTypeDesc = "--Select--";
            lstPaymentType.Add(defaultPaymenttype);
            lstPaymentType.AddRange(Enum.GetValues(typeof(Domain.Enumerations.PaymentType))
               .Cast<Domain.Enumerations.PaymentType>()
               .Select(ts => new PaymentTypeCA
               {
                   PaymentTypeId = ((int)ts),
                   PaymentTypeDesc = ts.ToString()
               }).ToList());
            return lstPaymentType;
        }

        private void GetPaymentPlans(List<InvoicePayment> Payments, List<Student> listStudents, ref List<PaymentPlanCA> lstpaymentplan)
        {
            if (Payments != null && Payments.Count > 0 && Payments[0].Invoice != null && Payments[0].PaymentType != PaymentType.None)
            {
                GetPaymentPlanList(listStudents, lstpaymentplan);

            }
        }

        private void GetPaymentPlanList(List<Student> listStudents, List<PaymentPlanCA> lstpaymentplan)
        {
            var paymentPlans =
                                _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));



            foreach (var data in paymentPlans)
            {
                if (data.TotalSessions > 1)
                    continue;
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
                lstpaymentplan.Add(paymentPlan);
            }
        }

        public List<StudentAttendanceDetails> GetStudentAttendanceInvoice(int invoiceid)
        {
            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            Student s = null;
            Session session = null;
            SessionRegister r = null;
            Teacher t = null;
            var query = Session.QueryOver<SessionAttendance>().
                JoinAlias(x => x.SubjectsAttended, () => ssa).
                JoinAlias(x => x.Student, () => s).
                JoinAlias(x => x.SessionRegister, () => r).
                JoinAlias(x => r.Session, () => session).
                JoinAlias(x => ssa.Teacher, () => t).
                Left.JoinAlias(x => x.Payments, () => pa).Where(x => x.Invoice.Id == invoiceid);

            List<SessionAttendance> list = new List<SessionAttendance>();
            if(invoiceid != 0)
            {
                list = query.List().ToList();
            }
            
            List<StudentAttendanceDetails> sList = new List<StudentAttendanceDetails>();

            if (invoiceid == 0)
                return sList;

            foreach (var sessionAttendance in list)
            {

                StudentAttendanceDetails studentAttendanceDetails = new StudentAttendanceDetails();
                studentAttendanceDetails.Date = sessionAttendance.SessionRegister.Date.ToString("yyyy-MM-dd");
                studentAttendanceDetails.InvoiceId = sessionAttendance.Invoice.Id;
                //Changes added -- Removed column
                //RemainingCredits = x.RemainingCredits,
                studentAttendanceDetails.SessionAttendanceId = sessionAttendance.Id;
                studentAttendanceDetails.Status = sessionAttendance.Status.ToString();

                List<Student> listStudents = new List<Student>();
                listStudents.Add(sessionAttendance.Student);



                List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
                PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
                defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
                lstpaymentplan.Add(defaultpaymentplan);
                GetPaymentPlanList(listStudents, lstpaymentplan);
                List<PaymentTypeCA> lstPaymentType = GetPaymentTypes();
                

                studentAttendanceDetails.lstPaymentPlan = lstpaymentplan;
                studentAttendanceDetails.PaymentPlanSelected = 0;
                studentAttendanceDetails.lstPaymentType = lstPaymentType;
                studentAttendanceDetails.PaymentTypeSelected = 0;

                if (invoiceid != 0)
                {
                    var count = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == sessionAttendance.Id).List().Count();
                    if (count > 0)
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                        var result = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == sessionAttendance.Id).List().First().Receipt.Id;
                        studentAttendanceDetails.RemainingCredits = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == result && x.Attendance != null).List().Count();
                    }
                    else
                        studentAttendanceDetails.IsDailyInvoiceData = "true";
                }
                else
                {
                    var paymentInvoice = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == sessionAttendance.Id).First();
                    if (paymentInvoice.NumberOfSessionsPaidFor > 1)
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "false";
                        studentAttendanceDetails.RemainingCredits = paymentInvoice.NumberOfSessionsPaidFor;
                    }
                    else
                    {
                        studentAttendanceDetails.IsDailyInvoiceData = "true";
                        studentAttendanceDetails.RemainingCredits = 0;
                    }
                }
                studentAttendanceDetails.Session = "Session On " + sessionAttendance.SessionRegister.Session.Day.ToString() + " at "
                                                + sessionAttendance.SessionRegister.Session.From.ToString() + "-" + sessionAttendance.SessionRegister.Session.To.ToString();
                studentAttendanceDetails.StudentName =
                    string.Format("{0} {1}", sessionAttendance.Student.FirstName, sessionAttendance.Student.LastName);
                studentAttendanceDetails.StudentNo = sessionAttendance.Student.StudentNo;
                studentAttendanceDetails.Teacher =
                    string.Format("{0}  {1}",
                                  sessionAttendance.SubjectsAttended.FirstOrDefault().Teacher.FirstName,
                                  sessionAttendance.SubjectsAttended.FirstOrDefault().Teacher.LastName);
                studentAttendanceDetails.PaymentDetails = sessionAttendance.Payments.Select(d => new StudentPaymentDetails()
                {
                    InvoiceNo =
                                                                    d.Invoice.Id.ToString(),
                    InvoiceType =
                                                                    d.Invoice.PaymentType.
                                                                    ToString(),
                    PaymentAmount =
                                                                   d.PaymentAmount.HasValue ? d.PaymentAmount.Value.ToString("C") : null,
                    PaymentType =
                                                                    d.PaymentType.ToString()
                }).ToList();

                sList.Add(studentAttendanceDetails);
            }
            return sList;
        }

        private void Get(List<Student> listStudents, List<PaymentPlanCA> lstpaymentplan)
        {
            var paymentPlans =
                                _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));

            foreach (var data in paymentPlans)
            {
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
                lstpaymentplan.Add(paymentPlan);
            }
        }

        public SessionAttendance GetSessionAttendance(int sessionAttendanceId)
        {
            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            Student s = null;
            Session session = null;
            SessionRegister r = null;
            Teacher t = null;
            var query = Session.QueryOver<SessionAttendance>().
                JoinAlias(x => x.SubjectsAttended, () => ssa).
                JoinAlias(x => x.Student, () => s).
                JoinAlias(x => x.SessionRegister, () => r).
                JoinAlias(x => r.Session, () => session).
                JoinAlias(x => ssa.Teacher, () => t).
                Left.JoinAlias(x => x.Payments, () => pa);

            return query.Where(x => x.Id == sessionAttendanceId).List().First();
        }

        public SearchAttendanceResult GetAttendances(SearchStudentAttendanceCriteria searchAttendanceCriterionCriteria)
        {
            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            Student s = null;
            Session session = null;
            SessionRegister r = null;
            Teacher t = null;
            var query = Session.QueryOver<SessionAttendance>().
                JoinAlias(x => x.SubjectsAttended, () => ssa).
                JoinAlias(x => x.Student, () => s).
                JoinAlias(x => x.SessionRegister, () => r).
                JoinAlias(x => r.Session, () => session).
                JoinAlias(x => ssa.Teacher, () => t);
                //Left.JoinAlias(x => x.Payments, () => pa);


            if (!string.IsNullOrEmpty(searchAttendanceCriterionCriteria.StudentNo))
            {
                query = query.Where(x => s.StudentNo == searchAttendanceCriterionCriteria.StudentNo);
            }

            if (!string.IsNullOrEmpty(searchAttendanceCriterionCriteria.FirstName))
            {
                query = query.Where(x => s.FirstName == searchAttendanceCriterionCriteria.FirstName);
            }

            if (!string.IsNullOrEmpty(searchAttendanceCriterionCriteria.LastName))
            {
                query = query.Where(x => s.LastName == searchAttendanceCriterionCriteria.LastName);
            }

            if (searchAttendanceCriterionCriteria.Dob.HasValue)
            {
                query = query.Where(x => s.DateOfBirth == searchAttendanceCriterionCriteria.Dob);
            }

            if (searchAttendanceCriterionCriteria.Curriculum.HasValue)
            {
                query = query.Where(x => (int)s.Curriculum == searchAttendanceCriterionCriteria.Curriculum);
            }

            if (searchAttendanceCriterionCriteria.SessionsFromDate.HasValue)
            {
                query = query.Where(x => r.Date >= searchAttendanceCriterionCriteria.SessionsFromDate);
            }

            if (searchAttendanceCriterionCriteria.SessionsToDate.HasValue)
            {
                query = query.Where(x => r.Date <= searchAttendanceCriterionCriteria.SessionsToDate);
            }

            
            

            IList<SessionAttendance> list =
                query.OrderBy(x => r.Date).Desc.Skip(searchAttendanceCriterionCriteria.PageIndex * searchAttendanceCriterionCriteria.PageSize)
                    .Take(searchAttendanceCriterionCriteria.PageSize).List();

            //IList<SessionAttendance> list =
            //    query.List();
            var rowCount = CriteriaTransformer.TransformToRowCount(query.UnderlyingCriteria)
                .UniqueResult<int>();

            var attendances = new List<StudentAttendanceDetails>();


            list.ToList().ForEach(x => attendances.Add(new StudentAttendanceDetails()
            {
                Date = x.SessionRegister.Date.ToString("yyyy-MM-dd"),
                //Changes added -- Removed column
                //RemainingCredits = x.RemainingCredits,
                SessionAttendanceId = x.Id,
                Status = x.Status.ToString(),
                Session =
                                                string.Format("{0}-{1}-{2}", x.SessionRegister.Session.Day,
                                                              x.SessionRegister.Session.From,
                                                              x.SessionRegister.Session.To),
                StudentName =
                                                string.Format("{0} {1}", x.Student.FirstName, x.Student.LastName),
                StudentNo = x.Student.StudentNo,
                Teacher =
                                                string.Format("{0}  {1}",
                                                              x.SubjectsAttended.FirstOrDefault().Teacher.FirstName,
                                                              x.SubjectsAttended.FirstOrDefault().Teacher.LastName),
                IsProcessed = x.SessionRegister.Status == Domain.Enumerations.SessionRegisterStatus.Processed ? true : false,
                
            }));

            foreach (var data in attendances)
            {
                #region payment
                var Payments = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == data.SessionAttendanceId && x.Invoice != null 
                && x.Invoice.Status != InvoiceStatus.Cancelled).ToList();

                data.PaymentDetails = Payments.Select(d => new StudentPaymentDetails()
                {
                    InvoiceNo =
                                                                    d.Invoice == null ? "0" : d.Invoice.Id.ToString(),
                    InvoiceType =
                                                                    d.Invoice == null ? "" : d.Invoice.PaymentType.
                                                                    ToString(),
                    PaymentAmount =
                                                                   d.PaymentAmount.HasValue ? d.PaymentAmount.Value.ToString("C") : null,
                    PaymentType =
                                                                    d.PaymentType.ToString(),
                }).ToList();
                data.InvoiceId = Payments != null && Payments.Count > 0 && Payments[0].Invoice != null ? Payments[0].Invoice.Id : 0;
                
                #endregion end payment region
                var sessionAttended = Session.Query<SessionAttendance>().Where(x => x.Id == data.SessionAttendanceId).First();
                List<Student> listStudents = new List<Student>();
                listStudents.Add(sessionAttended.Student);

                var paymentPlans =
                    _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));

                List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
                PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
                defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
                lstpaymentplan.Add(defaultpaymentplan);

                foreach (var dataPlan in paymentPlans)
                {
                    PaymentPlanCA paymentPlan = new PaymentPlanCA();
                    paymentPlan.PaymentPlanId = dataPlan.Id;
                    paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(dataPlan.Amount, 2).ToString();
                    lstpaymentplan.Add(paymentPlan);
                }
                List<PaymentTypeCA> lstPaymentType = new List<PaymentTypeCA>();
                PaymentTypeCA defaultPaymenttype = new PaymentTypeCA();
                defaultPaymenttype.PaymentTypeId = 0; defaultPaymenttype.PaymentTypeDesc = "--Select--";
                lstPaymentType.Add(defaultPaymenttype);
                lstPaymentType.AddRange(Enum.GetValues(typeof(Domain.Enumerations.PaymentType))
                   .Cast<Domain.Enumerations.PaymentType>()
                   .Select(ts => new PaymentTypeCA
                   {
                       PaymentTypeId = ((int)ts),
                       PaymentTypeDesc = ts.ToString()
                   }).ToList());

                data.lstPaymentPlan = lstpaymentplan;
                data.PaymentPlanSelected = 0;
                data.lstPaymentType = lstPaymentType;
                data.PaymentTypeSelected = 0;
                if (sessionAttended.Invoice != null)
                {
                    var invoiceRef = Session.Query<Invoice>()?.FirstOrDefault(x => x.Id == sessionAttended.Invoice.Id);
                    var invoicePaymentRef = Session.Query<PaymentReciept>()
                        ?.FirstOrDefault(x => x.Invoice.Id == invoiceRef.Id);
                    if (invoicePaymentRef == null)
                    {
                        var paymentInvoice = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == sessionAttended.Id).First();
                        if (paymentInvoice.NumberOfSessionsPaidFor > 1)
                        {
                            data.IsDailyInvoiceData = "false";
                            data.RemainingCredits = paymentInvoice.NumberOfSessionsPaidFor;
                        }
                        else
                        {
                            data.IsDailyInvoiceData = "true";
                            data.RemainingCredits = 0;
                        }
                    }
                    else
                    {
                        var count = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == invoicePaymentRef.Id)
                            .List().Count();
                        if (count > 0)
                        {
                            data.IsDailyInvoiceData = "false";
                            var result = Session.QueryOver<AttendanceCredit>()
                                .Where(x => x.Receipt.Id == invoicePaymentRef.Id).List().First().Receipt.Id;
                            data.RemainingCredits = Session.QueryOver<AttendanceCredit>()
                                .Where(x => x.Receipt.Id == result && x.Attendance == null).List().Count();
                        }
                        else
                            data.IsDailyInvoiceData = "true";
                    }
                }
                else
                {
                    var paymentInvoice = Session.Query<InvoicePayment>().Where(x => x.Attendance.Id == sessionAttended.Id).FirstOrDefault();
                    if (paymentInvoice != null)
                    {
                        if (paymentInvoice.NumberOfSessionsPaidFor > 1)
                        {
                            data.IsDailyInvoiceData = "false";
                            data.RemainingCredits = paymentInvoice.NumberOfSessionsPaidFor;
                        }
                        else
                        {
                            data.IsDailyInvoiceData = "true";
                            data.RemainingCredits = 0;
                        }
                    }
                    else
                    {
                        var student = Session.Query<Student>().Where(x => x.Id == sessionAttended.Student.Id).First();
                        var invoices = Session.Query<Invoice>().Where(x => x.Student.Id == student.Id).ToList();
                        bool isMatchFound = false;
                        foreach(var inv in invoices)
                        {
                            var invoicereceipt = Session.Query<PaymentReciept>().Where(x => x.Invoice.Id == inv.Id).FirstOrDefault();
                            if(invoicereceipt != null)
                            {
                                var attCredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invoicereceipt.Id && x.Attendance == null).ToList();
                                if(attCredit.Count > 0)
                                {
                                    data.IsDailyInvoiceData = "false";
                                    data.RemainingCredits = attCredit.Count;
                                    isMatchFound = true;
                                    break;
                                }
                                else
                                {
                                    continue;
                                }

                            }   
                        }
                        if(!isMatchFound)
                        {
                            var sessionAttendancesList = Session.Query<SessionAttendance>().Where(x => x.Student.Id == student.Id).ToList();
                            foreach(var saL in sessionAttendancesList)
                            {
                                var attCredit = Session.Query<AttendanceCredit>().Where(x => x.Attendance.Id == saL.Id).FirstOrDefault();
                                if(attCredit != null)
                                {
                                    var invReceipt = Session.Query<PaymentReciept>().Where(x => x.Id == attCredit.Receipt.Id).First();
                                    var tempSA = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invReceipt.Id && x.Attendance == null).ToList();
                                    if(tempSA.Count > 0)
                                    {
                                        data.IsDailyInvoiceData = "false";
                                        data.RemainingCredits = tempSA.Count;
                                        isMatchFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if(!isMatchFound)
                        {
                            data.RemainingCredits = 0;
                            data.IsDailyInvoiceData = "true";
                            isMatchFound = true;
                        }
                    }
                }

                if (data.RemainingCredits > 0)
                    data.RemainingCreditInStr = data.RemainingCredits.ToString();
                else
                    data.RemainingCreditInStr = "";

            }

            return new SearchAttendanceResult()
            {

                Attendances = attendances,
                MaxPageIndex = rowCount > 0 ? (int)Math.Ceiling((decimal)((rowCount - 1) / searchAttendanceCriterionCriteria.PageSize)) : 0,
                PageSize = searchAttendanceCriterionCriteria.PageSize
            };
        }

        public RegisterSearchResultDto SearchForSessionRegisters(SearchRegisterDto searchRegisterDto)
        {
            var query = Session.QueryOver<SessionRegister>();

            if (searchRegisterDto.CentreId.HasValue)
            {
                query = query.Where(x => x.Centre.Id == searchRegisterDto.CentreId);
            }

            if (searchRegisterDto.SessionId.HasValue)
            {
                query = query.Where(x => x.Session.Id == searchRegisterDto.SessionId);
            }

            DateTime fromDate;
            if (DateTime.TryParse(searchRegisterDto.FromDate, out fromDate))
            {
                query = query.Where(x => x.Date >= fromDate);
            }

            DateTime toDate;
            if (DateTime.TryParse(searchRegisterDto.ToDate, out toDate))
            {
                query = query.Where(x => x.Date <= toDate);
            }

            if (searchRegisterDto.Status.HasValue)
            {
                query = query.Where(x => x.Status == searchRegisterDto.Status);
            }

            IList<SessionRegister> list =
                query.OrderBy(x => x.Date).Desc.Skip(searchRegisterDto.PageIndex * searchRegisterDto.PageSize)
                    .Take(searchRegisterDto.PageSize).List();
            var rowCount = CriteriaTransformer.TransformToRowCount(query.UnderlyingCriteria)
                .UniqueResult<int>();


            return new RegisterSearchResultDto
            {
                Registers = Mapper.Map<List<RegisterSearchResultRow>>(list),
                MaxPageIndex = rowCount > 0 ? (int)Math.Ceiling((decimal)((rowCount - 1) / searchRegisterDto.PageSize)) : 0,
                PageSize = searchRegisterDto.PageSize
            };

        }
    }
}
