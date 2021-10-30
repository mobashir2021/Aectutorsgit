using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Domain.Extensions;
using AutoMapper;
using NHibernate;
using NHibernate.Linq;

namespace AECMIS.DAL.Nhibernate.Repositories
{
    public class InvoiceRepository : Repository<Invoice, int>
    {
        private readonly ISession _session;
        private readonly IRepository<PaymentPlan, int> _paymentPlanRepository;
        private readonly Func<List<Student>, List<Curriculum>> _studentCurriculums =
       list => list.Select(x => x.Curriculum).Distinct().ToList();
        public InvoiceRepository() : this(null)
        {
            _paymentPlanRepository = new Repository<PaymentPlan, int>();
        }

        public InvoiceRepository(ISession session)
        {
            _session = session ?? SessionManager.GetSession<Invoice>();
            _paymentPlanRepository = new Repository<PaymentPlan, int>();
        }

        protected override ISession Session
        {
            get { return _session; }
        }

        public CreateNewInvoiceViewModel GetNewInvoiceViewModel()
        {
            CreateNewInvoiceViewModel createNewInvoiceViewModel = new CreateNewInvoiceViewModel();
            var paymentplan = Session.Query<PaymentPlan>().ToList().Where(x => x.Amount != Convert.ToDecimal(150.00)).GroupBy(x => x.Amount).ToDictionary(x => x.Key, x => x.ToList());

            List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
            PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
            defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
            lstpaymentplan.Add(defaultpaymentplan);

            foreach (var data in paymentplan)
            {
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Value.First().Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + data.Value.First().Amount.ToString();
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

            createNewInvoiceViewModel.lstPaymentPlan = lstpaymentplan;
            createNewInvoiceViewModel.lstPaymentType = lstPaymentType;
            return createNewInvoiceViewModel;
        }

        public List<StudentAttendanceDetails> GetSessionAttendancesForGivenInvoice(int InvoiceId, bool isMakePayment = false)
        {

            SessionAttendance sa = null;
            SubjectAttendance ssa = null;
            InvoicePayment pa = null;
            Student s = null;
            Session session = null;
            SessionRegister r = null;
            Teacher t = null;
            var query = Session.QueryOver<SessionAttendance>().Where(x => x.Invoice.Id == InvoiceId).
                JoinAlias(x => x.SubjectsAttended, () => ssa).
                JoinAlias(x => x.Student, () => s).
                JoinAlias(x => x.SessionRegister, () => r).
                JoinAlias(x => r.Session, () => session).
                JoinAlias(x => ssa.Teacher, () => t).
                Left.JoinAlias(x => x.Payments, () => pa);


            var attendanceList = Session.QueryOver<SessionAttendance>().Where(x => x.Invoice.Id == InvoiceId && x.Status != SessionAttendanceStatus.AuthorizeAbsence).List();
            var currInvoice = Session.Query<Invoice>().Where(x => x.Id == InvoiceId)?.First();
            List<Student> listStudents = new List<Student>();
            listStudents.Add(currInvoice.Student);

            var attendances = new List<StudentAttendanceDetails>();
            var paymentPlans =
                _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));


            //var paymentplan = Session.Query<PaymentPlan>().ToList().Where(x => x.Amount != Convert.ToDecimal(150.00)).GroupBy(x => x.Amount).ToDictionary(x => x.Key, x => x.ToList());

            List<PaymentPlanCA> lstpaymentplan = new List<PaymentPlanCA>();
            //PaymentPlanCA defaultpaymentplan = new PaymentPlanCA();
            //defaultpaymentplan.PaymentPlanId = 0; defaultpaymentplan.PaymentPlanValue = "--Select--";
            //lstpaymentplan.Add(defaultpaymentplan);

            foreach (var data in paymentPlans)
            {
                if (data.TotalSessions > 1)
                    continue;
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
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
            if (isMakePayment)
            {
                lstPaymentType = lstPaymentType.Where(x => x.PaymentTypeDesc != "None").ToList();
            }

            foreach (var attendance in attendanceList)
            {
                GetInvoiceAttendanceDetails(attendance, attendances);
            }
            
            PopulateAttendanceInvoiceDetails(attendances, lstpaymentplan, lstPaymentType);

            return attendances;
        }

        private void PopulateAttendanceInvoiceDetails(List<StudentAttendanceDetails> attendances, List<PaymentPlanCA> lstpaymentplan, List<PaymentTypeCA> lstPaymentType)
        {
            int loopcount = 1;
            foreach (var data in attendances)
            {
                var fa = new FutureInvoiceAvailable
                {
                    FIAId = 0,
                    FIAValue = "Choose Future Invoice"
                };
                data.listFutureInvoices = new List<FutureInvoiceAvailable> {fa};


                var count = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == data.SessionAttendanceId).List()
                    .Count();
                if (count > 0)
                {
                    data.IsDailyInvoiceData = "false";
                    var result = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == data.SessionAttendanceId)
                        .List().First().Receipt.Id;
                    data.RemainingCredits = Session.QueryOver<AttendanceCredit>()
                        .Where(x => x.Receipt.Id == result && x.Attendance != null).List().Count();
                }
                else
                    data.IsDailyInvoiceData = "true";

                data.lstPaymentPlan = lstpaymentplan;
                data.PaymentPlanSelected = lstpaymentplan[0].PaymentPlanId;
                data.lstPaymentType = lstPaymentType;
                data.PaymentTypeSelected = 0;
                var studentId = Session.Query<Student>().Where(x => x.StudentNo == data.StudentNo)?.First()?.Id;
                var invoiceList = Session.Query<Invoice>().Where(x =>
                    x.Student.Id == studentId && data.InvoiceId != x.Id && x.NumberOfSessionsPayingFor > 1 &&
                    x.Status != InvoiceStatus.Cancelled && x.PaymentReciept != null).ToList();
                foreach (var invoice in invoiceList)
                {
                    if (invoice.PaymentReciept == null)
                        continue;
                    var attendanceCredit = Session.QueryOver<AttendanceCredit>()
                        .Where(x => x.Receipt.Id == invoice.PaymentReciept.Id && x.Attendance != null).List().ToList();
                    if (attendanceCredit.Count == invoice.NumberOfSessionsPayingFor)
                        continue;
                    var tempAttCredit = Session.QueryOver<AttendanceCredit>()
                        .Where(x => x.Receipt.Id == invoice.PaymentReciept.Id && x.Attendance == null).List().ToList();
                    int temploopvalue = 1;
                    foreach (var attObjValue in tempAttCredit)
                    {
                        var futureInvoiceObj = new FutureInvoiceAvailable();
                        futureInvoiceObj.FIAId = attObjValue.Id;
                        futureInvoiceObj.FIAValue =
                            "Credit " + temploopvalue.ToString() + " (Invoice " + invoice.Id.ToString() + ")";
                        temploopvalue = temploopvalue + 1;
                        data.listFutureInvoices.Add(futureInvoiceObj);
                    }
                }

                data.FutureInvoiceSelect = 0;
                loopcount += 1;
            }
        }

        private static void GetInvoiceAttendanceDetails(SessionAttendance attendance, List<StudentAttendanceDetails> attendances)
        {
            var attendanceDetails = new StudentAttendanceDetails()
            {
                Date = attendance.SessionRegister.Date.ToString("yyyy-MM-dd"),
                //Changes added -- Removed column
                //RemainingCredits = x.RemainingCredits,
                SessionAttendanceId = attendance.Id,
                Status = attendance.Status.ToString(),
                Session =
                    string.Format("{0}-{1}-{2}", attendance.SessionRegister.Session.Day,
                        attendance.SessionRegister.Session.From,
                        attendance.SessionRegister.Session.To),
                StudentName =
                    string.Format("{0} {1}", attendance.Student.FirstName, attendance.Student.LastName),
                StudentNo = attendance.Student.StudentNo,
                Teacher =
                    string.Format("{0}  {1}",
                        attendance.SubjectsAttended?.FirstOrDefault()?.Teacher?.FirstName,
                        attendance.SubjectsAttended?.FirstOrDefault()?.Teacher?.LastName),
                PaymentDetails = attendance.Payments.Select(d => new StudentPaymentDetails()
                {
                    InvoiceNo =
                        d.Invoice == null ? "0" : d.Invoice.Id.ToString(),
                    InvoiceType =
                        d.Invoice == null ? "" : d.Invoice.PaymentType.ToString(),
                    PaymentAmount =
                        d.PaymentAmount.HasValue ? d.PaymentAmount.Value.ToString("C") : null,
                    PaymentType =
                        d.PaymentType.ToString()
                }).ToList(),
                InvoiceId = attendance.Invoice != null ? attendance.Invoice.Id : 0
            };
            attendances.Add(attendanceDetails);
        }

        public List<FutureInvoiceAvailable> GetAvailableFutureInvoices(int invoiceId, string StudentNo)
        {
            List<FutureInvoiceAvailable> availableFutureInvoices = new List<FutureInvoiceAvailable>();
            var studentId = Session.Query<Student>().Where(x => x.StudentNo == StudentNo)?.First()?.Id;
            FutureInvoiceAvailable fa = new FutureInvoiceAvailable();
            fa.FIAId = 0; fa.FIAValue = "Choose Future Invoice";
            availableFutureInvoices.Add(fa);
            var tempInvoices = Session.Query<Invoice>().Where(x => x.Student.Id == studentId && invoiceId != x.Id && x.Status != InvoiceStatus.Cancelled && x.NumberOfSessionsPayingFor > 1 && x.PaymentReciept != null).ToList();
            foreach (var tempInvoice in tempInvoices)
            {
                if (tempInvoice.PaymentReciept == null)
                    continue;
                //if (count == 0)
                //    continue;
                var attendanceCredit = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == tempInvoice.PaymentReciept.Id && x.Attendance != null).List().ToList();
                if (attendanceCredit.Count == tempInvoice.NumberOfSessionsPayingFor)
                    continue;
                var tempAttCredit = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == tempInvoice.PaymentReciept.Id && x.Attendance == null).List().ToList();
                int temploopvalue = 1;
                foreach (var attObjValue in tempAttCredit)
                {
                    var futureInvoiceObj = new FutureInvoiceAvailable();
                    futureInvoiceObj.FIAId = attObjValue.Id; futureInvoiceObj.FIAValue = "Credit " + temploopvalue.ToString() + " (Invoice " + tempInvoice.Id.ToString() + ")";
                    temploopvalue = temploopvalue + 1;
                    availableFutureInvoices.Add(futureInvoiceObj);
                }
                
            }
            return availableFutureInvoices;
        }

        public List<StudentAttendanceDetails> GetSessionAttendancesWithInvoice(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            bool isInvalidateEnabled = false;
            var attInvoiceId = Session.QueryOver<SessionAttendance>().Where(x => x.Id == invalidateInvoiceViewModel.SessionAttendanceId).List().First().Invoice.Id;
           
            var attendances = new List<StudentAttendanceDetails>();
            var curInvoice = Session.Query<Invoice>().Where(x => x.Id == attInvoiceId)?.First();
            List<Student> listStudents = new List<Student>();
            listStudents.Add(curInvoice.Student);

            var paymentPlanList = GetPaymentPlanList(listStudents, out var lstPaymentType);

            int loopcount = 1;
            foreach (var data in invalidateInvoiceViewModel.listStudentAttendanceDetails)
            {
                List<FutureInvoiceAvailable> lstFuture = new List<FutureInvoiceAvailable>();
                FutureInvoiceAvailable fa = new FutureInvoiceAvailable();
                fa.FIAId = 0; fa.FIAValue = "Choose Future Invoice";
                lstFuture.Add(fa);
                var count = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == data.SessionAttendanceId).List().Count();
                if (count > 0)
                {
                    data.IsDailyInvoiceData = "false";
                    var result = Session.QueryOver<AttendanceCredit>().Where(x => x.Attendance.Id == data.SessionAttendanceId).List().First().Receipt.Id;
                    data.RemainingCredits = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == result && x.Attendance != null).List().Count();
                }
                else
                    data.IsDailyInvoiceData = "true";

                data.lstPaymentPlan = paymentPlanList;
                if (data.SessionAttendanceId == invalidateInvoiceViewModel.SessionAttendanceId)
                {
                    data.PaymentPlanSelected = invalidateInvoiceViewModel.PaymentPlanSelectedInv;
                    data.PaymentTypeSelected = invalidateInvoiceViewModel.PaymentTypeSelectedInv;
                    data.ChequeNo = invalidateInvoiceViewModel.ChequeNoInv;
                    data.PaymentDate = invalidateInvoiceViewModel.PaymentDateInv;
                    data.PaymentDateInStr = invalidateInvoiceViewModel.PaymentDateInv.ToString();
                }

                data.lstPaymentType = lstPaymentType;
                GetFutureInvoiceIfExists(data);

                attendances.Add(data);
                loopcount += 1;
            }


            return attendances;
        }

        private List<PaymentPlanCA> GetPaymentPlanList(List<Student> listStudents, out List<PaymentTypeCA> lstPaymentType)
        {
            var paymentPlans =
                _paymentPlanRepository.QueryList(x => _studentCurriculums(listStudents).Contains(x.Curriculum));

            var paymentPlanList = new List<PaymentPlanCA>();

            foreach (var data in paymentPlans)
            {
                if (data.TotalSessions > 1)
                    continue;
                PaymentPlanCA paymentPlan = new PaymentPlanCA();
                paymentPlan.PaymentPlanId = data.Id;
                paymentPlan.PaymentPlanValue = "\u00A3" + Math.Round(data.Amount, 2).ToString();
                paymentPlanList.Add(paymentPlan);
            }

            lstPaymentType = new List<PaymentTypeCA>();
            PaymentTypeCA defaultPaymenttype = new PaymentTypeCA();
            defaultPaymenttype.PaymentTypeId = 0;
            defaultPaymenttype.PaymentTypeDesc = "--Select--";
            lstPaymentType.Add(defaultPaymenttype);
            lstPaymentType.AddRange(Enum.GetValues(typeof(Domain.Enumerations.PaymentType))
                .Cast<Domain.Enumerations.PaymentType>()
                .Select(ts => new PaymentTypeCA
                {
                    PaymentTypeId = ((int) ts),
                    PaymentTypeDesc = ts.ToString()
                }).ToList());
            return paymentPlanList;
        }

        private void GetFutureInvoiceIfExists(StudentAttendanceDetails data)
        {
            var studentid = Session.Query<Student>().Where(x => x.StudentNo == data.StudentNo).First().Id;
            var temp = Session.Query<Invoice>().Where(x => x.Student.Id == studentid && data.InvoiceId != x.Id && x.Status != InvoiceStatus.Cancelled && x.NumberOfSessionsPayingFor > 1 && x.PaymentReciept != null).ToList();
            foreach (var tempinvoice in temp)
            {
                if (tempinvoice.PaymentReciept == null)
                    continue;
                //if (count == 0)
                //    continue;
                var attendancecredit = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == tempinvoice.PaymentReciept.Id && x.Attendance != null).List().ToList();
                if (attendancecredit.Count == tempinvoice.NumberOfSessionsPayingFor)
                    continue;
                var tempattcredit = Session.QueryOver<AttendanceCredit>().Where(x => x.Receipt.Id == tempinvoice.PaymentReciept.Id && x.Attendance == null).List().ToList();
                int temploopvalue = 1;
                if(data.listFutureInvoices!=null)
                { data.listFutureInvoices.Clear();}
                else
                {
                    data.listFutureInvoices=new List<FutureInvoiceAvailable>();
                }
                FutureInvoiceAvailable facurr = new FutureInvoiceAvailable();
                facurr.FIAId = 0; facurr.FIAValue = "Choose Future Invoice";
                data.listFutureInvoices.Add(facurr);
                foreach (var attObjValue in tempattcredit)
                {
                    FutureInvoiceAvailable futureInvoiceObj = new FutureInvoiceAvailable();
                    futureInvoiceObj.FIAId = attObjValue.Id; futureInvoiceObj.FIAValue = "Credit " + temploopvalue.ToString() + " (Invoice " + tempinvoice.Id.ToString() + ")";
                    temploopvalue = temploopvalue + 1;
                    data.listFutureInvoices.Add(futureInvoiceObj);
                }

            }
        }

        public void MakePaymentForInvoice(int InvoiceId, int PaymentType, int PaymentPlan, DateTime PaymentDate, string ChequeNo)
        {
            var paymentPlans = Session.Query<PaymentPlan>().Where(x => x.Id == PaymentPlan)?.First();
            var oldInvoice = Session.Query<Invoice>().Where(x => x.Id == InvoiceId)?.First();
            oldInvoice.TotalAmount = paymentPlans.Amount;
            oldInvoice.DiscountApplied = 0;
            oldInvoice.TotalAfterDiscount = oldInvoice.TotalAmount - oldInvoice.DiscountApplied.Value;
            var oldInvoiceId = oldInvoice.Id;
            var sessionAttendance = Session.QueryOver<SessionAttendance>().Where(x => x.Invoice.Id == oldInvoiceId).List();

            foreach (var data in sessionAttendance)
            {
                var invoicePayment = GetInvoicePaymentDetails(PaymentType, PaymentPlan, PaymentDate, ChequeNo, oldInvoice, data, paymentPlans);

                using (var tx = this.Session.BeginTransaction())
                {
                    Session.Flush();
                    oldInvoice = CalculateVATAndSequenceNumber(oldInvoice);
                    Session.SaveOrUpdate(oldInvoice);

                    Session.Flush();
                    Session.Save(invoicePayment);
                    PaymentReciept paymentReciept = new PaymentReciept();
                    paymentReciept.Invoice = oldInvoice;
                    paymentReciept.InvoicePayment = invoicePayment;
                    paymentReciept.GeneratedDate = DateTime.Now;
                    Session.Flush();
                    int paymentrecieptid = (int)Session.Save(paymentReciept);
                    if(oldInvoice.NumberOfSessionsPayingFor > 1)
                    {
                        for (int i = 0; i < oldInvoice.NumberOfSessionsPayingFor; i++)
                        {
                            AttendanceCredit attendanceCredit = new AttendanceCredit();
                            if ((data.Status == SessionAttendanceStatus.Attended || data.Status == SessionAttendanceStatus.UnAuthorizedAbsence) && i == 0)
                                attendanceCredit.Attendance = data;
                            attendanceCredit.Receipt = paymentReciept;
                            Session.Flush();
                            Session.SaveOrUpdate(attendanceCredit);
                        }
                    }
                    invoicePayment.Reciept = paymentReciept;
                    Session.Flush();
                    Session.SaveOrUpdate(invoicePayment);
                    tx.Commit();
                }
            }
        }

        private InvoicePayment GetInvoicePaymentDetails(int PaymentType, int PaymentPlan, DateTime PaymentDate, string ChequeNo,
            Invoice oldInvoice, SessionAttendance data, PaymentPlan paymentPlans)
        {
            var invoicePayment = Session.Query<InvoicePayment>().Where(x => x.Invoice.Id == oldInvoice.Id)?.FirstOrDefault();
            if (invoicePayment == null)
            {
                invoicePayment = new InvoicePayment();
                invoicePayment.Attendance = data;
                invoicePayment.Invoice = oldInvoice;
            }

            invoicePayment.NumberOfSessionsPaidFor = paymentPlans.TotalSessions;
            var paymentplan = Session.QueryOver<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(PaymentPlan)).List().First();
            //convert payment type id to enum equivalent
            invoicePayment.PaymentType = PaymentType.ToEnum<PaymentType>();
            invoicePayment.PaymentAmount = paymentplan.Amount;
            invoicePayment.PaymentDate = Convert.ToDateTime(PaymentDate);
            invoicePayment.ChequeNo = ChequeNo;
            oldInvoice.NumberOfSessionsPayingFor = paymentPlans.TotalSessions;
            oldInvoice.Status = InvoiceStatus.Paid;
            oldInvoice.PaymentType = paymentPlans.TotalSessions > 1 ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            return invoicePayment;
        }

        public InvoiceViewModel GetWithVAT(InvoiceViewModel invoicetemp)
        {
            var invoice = Session.Query<Invoice>().Where(x => x.Id == invoicetemp.Id).First();
            var vatdetails = Session.QueryOver<VATDetails>().List().ToList();
            //invoicetemp.Gross = invoice.TotalAmount.ToString();
            invoicetemp.Gross = "\u00A3" + Math.Round((invoice.TotalAfterDiscount - invoice.VATAmount), 2);
            var query = vatdetails.Where(x => invoice.CreatedDate <= x.ToDate && invoice.CreatedDate >= x.FromDate).FirstOrDefault();
            var vatPercentage = query?.VATPercentage ?? 20;
            if (query != null)
            {
                //decimal totalvalueVATAD = invoice.TotalAfterDiscount - (invoice.TotalAfterDiscount / Convert.ToDecimal(1.2));
                if (invoice.VATAmount != Convert.ToDecimal(0))
                    invoicetemp.VATAmount = invoice.VATAmount.ToString();
                else
                    invoicetemp.VATAmount = "\u00A3" + Convert.ToString( Math.Round(invoice.TotalAfterDiscount - ((invoice.TotalAfterDiscount / (100m + vatPercentage)) * 100m), 2));
                invoicetemp.VATLabel = "VAT @ " + Math.Round(vatPercentage,2).ToString() + "% : ";
                invoicetemp.NetAmount = invoice.TotalAfterDiscount.ToString();
            }

            return invoicetemp;
        }

        public void InvalidateEmptyInvoice(int id)
        {

        }

        public void SaveInvalidateInvoice(InvalidateInvoiceViewModel invalidate)
        {
            UpdateInvalidatedInvoiceAttendanceDetails(invalidate);
            using (var tx = this.Session.BeginTransaction())
            {
                Invoice tempOldInvoice;
                if (invalidate.listStudentAttendanceDetails != null && invalidate.listStudentAttendanceDetails.Count > 0)
                {
                    tempOldInvoice = InvalidateInvoiceWhenAttendancesExist(invalidate);
                }
                else
                {
                    tempOldInvoice = InvalidateInvoiceWhenAttendanceDoesNotExist(invalidate);
                }

                AddCreditNotes(invalidate, tempOldInvoice);
                tx.Commit();
            }
        }

        private Invoice InvalidateInvoiceWhenAttendanceDoesNotExist(InvalidateInvoiceViewModel invalidate)
        {
            var tempOldInvoice = Session.Query<Invoice>().Where(x => x.Id == invalidate.Id)?.FirstOrDefault();
            if (tempOldInvoice != null)
            {
                if (tempOldInvoice.PaymentReciept != null)
                {
                    var attCredit = Session.Query<AttendanceCredit>()
                        .Where(x => x.Receipt.Id == tempOldInvoice.PaymentReciept.Id).ToList();
                    foreach (var credit in attCredit)
                    {
                        Session.Flush();
                        Session.Delete(credit);
                    }
                }

                tempOldInvoice.Status = InvoiceStatus.Cancelled;
                Session.Flush();
                Session.SaveOrUpdate(tempOldInvoice);
            }

            return tempOldInvoice;
        }

        private Invoice InvalidateInvoiceWhenAttendancesExist(InvalidateInvoiceViewModel invalidate)
        {
            foreach (var data in invalidate.listStudentAttendanceDetails)
            {
                var sessionAttendance =
                    Session.Query<SessionAttendance>().Where(x => x.Id == data.SessionAttendanceId)?.First();
                if (data.FutureInvoiceSelect > 0 || data.SelectedValue == (int) SessionAttendanceStatus.AuthorizeAbsence)
                {
                    if (data.InvoiceId > 0)
                    {
                        CancelInvoiceIfInvoiceIsPresent(data, sessionAttendance);
                    }
                }
                else
                {
                    var planSelected = Session.Query<PaymentPlan>().Where(x => x.Id == data.PaymentPlanSelected)
                        ?.FirstOrDefault();
                    var Plan = (planSelected != null && planSelected.TotalSessions > 1) ? "Advanced" : "Daily";
                    if (data.IsDailyInvoiceData == "true")
                    {
                        if (data.FutureInvoiceSelect > 0)
                        {
                            CancelIfFutureInvoiceAvailable(data, sessionAttendance);
                        }
                        else if (data.PaymentTypeSelected > 0)
                        {
                            CancelAndCreateNewInvoiceIfNewInvoiceSelected(data, sessionAttendance, planSelected, Plan);
                        }
                    }
                    else
                    {
                        var paymentReceipTemp = new PaymentReciept();
                        var oldinvoice = Session.Query<Invoice>().Where(x => x.Id == data.InvoiceId)?.First();
                        var attendanceCreditList = new List<AttendanceCredit>();
                        if (oldinvoice.PaymentReciept.Id > 0)
                            paymentReceipTemp = Session.QueryOver<PaymentReciept>()
                                .Where(x => x.Id == oldinvoice.PaymentReciept.Id).List().FirstOrDefault();
                        if (paymentReceipTemp.Id > 0)
                            attendanceCreditList = Session.Query<AttendanceCredit>()
                                .Where(x => x.Receipt.Id == paymentReceipTemp.Id).ToList();
                        Session.Flush();
                        foreach (var attCredit in attendanceCreditList)
                        {
                            Session.Flush();
                            Session.Delete(attCredit);
                        }

                        if (data.FutureInvoiceSelect < 1)
                        {
                            oldinvoice =
                                InvalidateOldInvoiceAndCreateNewInvoice(data, sessionAttendance, planSelected, oldinvoice);
                        }
                    }
                }

                sessionAttendance.Status = data.SelectedValue == (int) SessionAttendanceStatus.Attended
                    ? SessionAttendanceStatus.Attended
                    : (data.SelectedValue == (int) SessionAttendanceStatus.AuthorizeAbsence
                        ? SessionAttendanceStatus.AuthorizeAbsence
                        : SessionAttendanceStatus.UnAuthorizedAbsence);
                if (sessionAttendance.Status == SessionAttendanceStatus.AuthorizeAbsence)
                {
                    sessionAttendance.Status = SessionAttendanceStatus.AuthorizeAbsence;
                    Session.Flush();
                    Session.SaveOrUpdate(sessionAttendance);
                }
            }

            var attendanceDetails = invalidate.listStudentAttendanceDetails[0];
           var tempOldInvoice = Session.Query<Invoice>().Where(x => x.Id == attendanceDetails.InvoiceId)?.First();
            tempOldInvoice.Status = InvoiceStatus.Cancelled;
            Session.Flush();
            Session.SaveOrUpdate(tempOldInvoice);
            return tempOldInvoice;
        }

        private void UpdateInvalidatedInvoiceAttendanceDetails(InvalidateInvoiceViewModel invalidate)
        {
            if (invalidate.listStudentAttendanceDetails != null)
            {
                foreach (var attended in invalidate.listStudentAttendanceDetails)
                {
                    if (attended.FutureInvoiceSelect > 0)
                    {
                        var paymentObj = Session.Query<AttendanceCredit>().Where(x => x.Id == attended.FutureInvoiceSelect)
                            .FirstOrDefault();
                        if (paymentObj != null && paymentObj.Receipt != null)
                        {
                            var invoiceObj = Session.Query<Invoice>().Where(x => x.PaymentReciept.Id == paymentObj.Receipt.Id)
                                .FirstOrDefault();
                            if (invoiceObj != null)
                                attended.FutureInvoiceSelect = invoiceObj.Id;
                            else
                                attended.FutureInvoiceSelect = 0;
                        }
                        else
                            attended.FutureInvoiceSelect = 0;
                    }
                }
            }
        }

        private void AddCreditNotes(InvalidateInvoiceViewModel invalidate, Invoice tempoldinvoice)
        {
            CreditNote creditNote = new CreditNote();
            creditNote.Notes = invalidate.CreditNote; creditNote.Invoice = tempoldinvoice;
            creditNote.CreatedDate = DateTime.Now;
            creditNote = CalculateCreditNoteSequenceNo(creditNote);
            Session.Flush();
            Session.SaveOrUpdate(creditNote);
        }

        private Invoice InvalidateOldInvoiceAndCreateNewInvoice(StudentAttendanceDetails data, SessionAttendance sessionattendance, PaymentPlan PlanSelected, Invoice oldinvoice)
        {
            oldinvoice.Status = InvoiceStatus.Cancelled;
            var student = Session.Query<Student>().Where(x => x.StudentNo == data.StudentNo).First();

            InvoicePayment invoicePayment = AddInvoicePayment(data, sessionattendance);
            Invoice invoice = AssignNewInvoice(data, PlanSelected, student, invoicePayment);

            PaymentReciept paymentReciept = new PaymentReciept();
            paymentReciept.Invoice = invoice;
            if (invoicePayment.PaymentType != PaymentType.None)
            {
                paymentReciept.InvoicePayment = invoicePayment;
                //paymentReciept.CreatedBy = 2; paymentReciept.ModifiedBy = 2; paymentReciept.CreatedDate = DateTime.Now; paymentReciept.ModifiedDate = DateTime.Now;
                paymentReciept.GeneratedDate = DateTime.Now;
                Session.Flush();
                int paymentrecieptid = (int)Session.Save(paymentReciept);
            }
            invoicePayment.Invoice = invoice;
            invoicePayment.Reciept = invoicePayment.PaymentType != PaymentType.None ? paymentReciept : null;
            Session.Flush();
            Session.SaveOrUpdate(invoicePayment);

            sessionattendance.Invoice = invoice;
            Session.Flush();
            Session.SaveOrUpdate(sessionattendance);

            Session.Flush();
            oldinvoice = CalculateVATAndSequenceNumber(oldinvoice);
            Session.SaveOrUpdate(oldinvoice);
            return oldinvoice;
        }

        private Invoice AssignNewInvoice(StudentAttendanceDetails data, PaymentPlan PlanSelected, Student student, InvoicePayment invoicePayment)
        {
            Invoice invoice = new Invoice();
            //invoice.CreatedBy = 2; invoice.ModifiedBy = 2; invoice.CreatedDate = DateTime.Now; invoice.ModifiedDate = DateTime.Now;
            invoice.NumberOfSessionsPayingFor = PlanSelected != null ? PlanSelected.TotalSessions : 0;
            DateTime validatedatetime = !string.IsNullOrEmpty(data.PaymentDateInStr) ? Convert.ToDateTime(data.PaymentDateInStr) : DateTime.Now;
            bool isvaliddatetime = validatedatetime.Year > 1;

            invoice.DateOfGeneration = (!string.IsNullOrEmpty(data.PaymentDateInStr) && isvaliddatetime) ? Convert.ToDateTime(data.PaymentDateInStr) : DateTime.Now;
            invoice.Status = invoicePayment.PaymentType != PaymentType.None ? InvoiceStatus.Paid : InvoiceStatus.Pending;
            invoice.Student = student;
            invoice.TotalAmount = invoicePayment.PaymentAmount.HasValue ? (decimal)invoicePayment.PaymentAmount : Convert.ToDecimal(0);
            invoice.DiscountApplied = Convert.ToDecimal(0);
            //invoice.
            invoice.PaymentType = (PlanSelected != null && PlanSelected.TotalSessions > 1) ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            invoice.TotalAfterDiscount = invoice.TotalAmount - invoice.DiscountApplied.Value;
            //if payment type is none 
            //convert invoice to daily invoice
            UpdateInvoiceDetailsWhenPaymentIsNone(invoice, student);
            invoice = CalculateVATAndSequenceNumber(invoice);
            invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            Session.Flush();
            int invoiceid = (int)Session.Save(invoice);
            return invoice;
        }

        private InvoicePayment AddInvoicePayment(StudentAttendanceDetails data, SessionAttendance sessionattendance)
        {
            InvoicePayment invoicePayment = new InvoicePayment();
            invoicePayment.NumberOfSessionsPaidFor = 1;
            invoicePayment.Attendance = sessionattendance;
            invoicePayment.PaymentDate = !string.IsNullOrEmpty(data.PaymentDateInStr) ? Convert.ToDateTime(data.PaymentDateInStr) : invoicePayment.PaymentDate;
            var paymentplan = Session.QueryOver<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(data.PaymentPlanSelected)).List().FirstOrDefault();
            //var paymenttype = (PaymentType)System.Enum.Parse(typeof(PaymentType), data.PaymentTypeSelected == 3001 ? "Cash" : (data.PaymentTypeSelected == 3002 ? "Cheque" : "None"));
            invoicePayment.PaymentType = data.PaymentTypeSelected.ToEnum<PaymentType>(); //paymenttype;
            if (invoicePayment.PaymentType != PaymentType.None)
                invoicePayment.PaymentAmount = paymentplan?.Amount;
            else
                invoicePayment.PaymentAmount = null;
            invoicePayment.ChequeNo = data.ChequeNo;
            sessionattendance.Status = data.SelectedValue == 1001 ? SessionAttendanceStatus.Attended : (data.SelectedValue == 1002 ? SessionAttendanceStatus.AuthorizeAbsence : SessionAttendanceStatus.UnAuthorizedAbsence);
            //invoicePayment.CreatedBy = 2; invoicePayment.ModifiedBy = 2;
            //invoicePayment.CreatedDate = DateTime.Now; invoicePayment.ModifiedDate = DateTime.Now;
            Session.Flush();
            int paymentid = (int)Session.Save(invoicePayment);
            return invoicePayment;
        }

        private void CancelAndCreateNewInvoiceIfNewInvoiceSelected(StudentAttendanceDetails data, SessionAttendance sessionattendance, PaymentPlan PlanSelected, string Plan)
        {
            var oldinvoice = Session.Query<Invoice>().Where(x => x.Id == data.InvoiceId).First();
            oldinvoice.Status = InvoiceStatus.Cancelled;
            oldinvoice = CalculateVATAndSequenceNumber(oldinvoice);
            if (oldinvoice.PaymentReciept != null)
            {
                var attcredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == oldinvoice.PaymentReciept.Id).ToList();
                foreach (var credit in attcredit)
                {
                    Session.Flush();
                    Session.Delete(credit);
                }
            }
            var student = Session.Query<Student>().Where(x => x.StudentNo == data.StudentNo).First();
            var sessionAttendance = Session.QueryOver<SessionAttendance>().Where(x => x.Id == data.SessionAttendanceId).List().First();
            InvoicePayment invoicePayment;
            PaymentPlan paymentplan;
            AddInvoicePaymentWithPaymentPlan(data, sessionattendance, PlanSelected, sessionAttendance, out invoicePayment, out paymentplan);
            Invoice invoice = CreateNewInvoiceIfDailyInvoiceRequiredInInvalidate(data, Plan, student, invoicePayment, paymentplan);

            PaymentReciept paymentReciept = new PaymentReciept();
            paymentReciept.Invoice = invoice;
            if (invoicePayment.PaymentType != PaymentType.None)
            {
                paymentReciept.InvoicePayment = invoicePayment;

                paymentReciept.GeneratedDate = DateTime.Now;
                Session.Flush();
                int paymentrecieptid = (int)Session.Save(paymentReciept);
            }
            invoicePayment.Invoice = invoice;

            Session.Flush();
            Session.SaveOrUpdate(invoicePayment);



            sessionattendance.Invoice = invoice;
            Session.Flush();
            Session.SaveOrUpdate(sessionattendance);
            Session.Flush();
            if (Plan == "Advanced")
            {
                for (int i = 1; i <= PlanSelected.TotalSessions; i++)
                {
                    AttendanceCredit attendanceCredit = new AttendanceCredit();
                    if (i == 1)
                        attendanceCredit.Attendance = sessionAttendance;
                    attendanceCredit.Receipt = paymentReciept;
                    Session.SaveOrUpdate(attendanceCredit);
                }
            }

            Session.Flush();
            Session.SaveOrUpdate(oldinvoice);
        }

        private Invoice CreateNewInvoiceIfDailyInvoiceRequiredInInvalidate(StudentAttendanceDetails data, string Plan, Student student, InvoicePayment invoicePayment, PaymentPlan paymentplan)
        {
            Invoice invoice = new Invoice();

            invoice.NumberOfSessionsPayingFor = paymentplan != null ? paymentplan.TotalSessions : 0;

            DateTime validatedatetime = !string.IsNullOrEmpty(data.PaymentDateInStr) ? Convert.ToDateTime(data.PaymentDateInStr) : DateTime.Now;
            bool isvaliddatetime = validatedatetime.Year > 1;

            invoice.DateOfGeneration = (!string.IsNullOrEmpty(data.PaymentDateInStr) && isvaliddatetime) ? Convert.ToDateTime(data.PaymentDateInStr) : DateTime.Now;
            invoice.Status = invoicePayment.PaymentType != PaymentType.None ? InvoiceStatus.Paid : InvoiceStatus.Pending;
            invoice.Student = student;
            if (invoicePayment.PaymentAmount != null)
                invoice.TotalAmount = (decimal)invoicePayment.PaymentAmount;
            invoice.PaymentType = Plan == "Advanced" ? InvoicePaymentType.Advanced : InvoicePaymentType.Daily;
            invoice.DiscountApplied = 0;
            invoice.TotalAfterDiscount = invoice.TotalAmount - invoice.DiscountApplied.Value;
            
            //if payment type is none 
            //convert invoice to daily invoice
            UpdateInvoiceDetailsWhenPaymentIsNone(invoice, student);
            invoice = CalculateVATAndSequenceNumber(invoice);
            //invoice.TotalAfterDiscount = invoice.TotalAfterDiscount;
            Session.Flush();
            int invoiceid = (int)Session.Save(invoice);
            return invoice;
        }

        private void AddInvoicePaymentWithPaymentPlan(StudentAttendanceDetails data, SessionAttendance sessionattendance, PaymentPlan PlanSelected, SessionAttendance sessionAttendance, out InvoicePayment invoicePayment, out PaymentPlan paymentplan)
        {
            invoicePayment = new InvoicePayment();
            invoicePayment.NumberOfSessionsPaidFor = PlanSelected != null ? PlanSelected.TotalSessions : 0;
            invoicePayment.Attendance = sessionAttendance;
            invoicePayment.PaymentDate = !string.IsNullOrEmpty(data.PaymentDateInStr) ? Convert.ToDateTime(data.PaymentDateInStr) : invoicePayment.PaymentDate;
            paymentplan = Session.QueryOver<PaymentPlan>().Where(x => x.Id == Convert.ToInt32(data.PaymentPlanSelected)).List().FirstOrDefault();
            //var paymenttype = (PaymentType)System.Enum.Parse(typeof(PaymentType), data.PaymentTypeSelected == (int)PaymentType.Cash ? "Cash" : (data.PaymentTypeSelected == (int)PaymentType.Cheque ? "Cheque" : "None"));
            invoicePayment.PaymentType = data.PaymentTypeSelected.ToEnum< PaymentType>();
            if (invoicePayment.PaymentType == PaymentType.None || paymentplan == null)
                invoicePayment.PaymentAmount = null;
            else
                invoicePayment.PaymentAmount = paymentplan.Amount;

            invoicePayment.ChequeNo = data.ChequeNo;
            sessionattendance.Status = data.SelectedValue == (int)SessionAttendanceStatus.Attended ? SessionAttendanceStatus.Attended : (data.SelectedValue == (int)SessionAttendanceStatus.AuthorizeAbsence ? SessionAttendanceStatus.AuthorizeAbsence : SessionAttendanceStatus.UnAuthorizedAbsence);

            Session.Flush();
            int paymentid = (int)Session.Save(invoicePayment);
        }

        private void CancelIfFutureInvoiceAvailable(StudentAttendanceDetails data, SessionAttendance sessionattendance)
        {
            var oldinvoice = Session.Query<Invoice>().Where(x => x.Id == data.InvoiceId).First();
            if (oldinvoice.PaymentReciept != null)
            {
                var attcredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == oldinvoice.PaymentReciept.Id).ToList();
                foreach (var credit in attcredit)
                {
                    Session.Flush();
                    Session.Delete(credit);
                }
            }
            var newinvoice = Session.Query<Invoice>().Where(x => x.Id == data.FutureInvoiceSelect).First();
            var paymentreceipt = Session.QueryOver<PaymentReciept>().Where(x => x.Id == newinvoice.PaymentReciept.Id).List().First();
            sessionattendance.Status = data.SelectedValue == (int)SessionAttendanceStatus.Attended ? SessionAttendanceStatus.Attended : (data.SelectedValue == (int)SessionAttendanceStatus.AuthorizeAbsence ? SessionAttendanceStatus.AuthorizeAbsence : SessionAttendanceStatus.UnAuthorizedAbsence);
            sessionattendance.Invoice = newinvoice;
            var attendancecredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == paymentreceipt.Id).First();
            oldinvoice.Status = Domain.Enumerations.InvoiceStatus.Cancelled;
            oldinvoice = CalculateVATAndSequenceNumber(oldinvoice);
            Session.Flush();
            Session.SaveOrUpdate(sessionattendance);
            Session.SaveOrUpdate(oldinvoice);
        }

        private void CancelInvoiceIfInvoiceIsPresent(StudentAttendanceDetails data, SessionAttendance sessionattendance)
        {
            var invoicetemp = Session.Query<Invoice>().Where(x => x.Id == data.InvoiceId).FirstOrDefault();
            if (invoicetemp != null)
            {
                if (invoicetemp.PaymentReciept != null)
                {
                    var attcredit = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == invoicetemp.PaymentReciept.Id).ToList();
                    foreach (var credit in attcredit)
                    {
                        Session.Flush();
                        Session.Delete(credit);
                    }
                }
                invoicetemp.Status = InvoiceStatus.Cancelled;
                Session.Flush();
                Session.SaveOrUpdate(invoicetemp);
            }
            var newInvoiceFuture = Session.Query<Invoice>().Where(x => x.Id == data.FutureInvoiceSelect).FirstOrDefault();
            if (newInvoiceFuture != null)
            {
                if (newInvoiceFuture.PaymentReciept != null)
                {
                    var attcreditFuture = Session.Query<AttendanceCredit>().Where(x => x.Receipt.Id == newInvoiceFuture.PaymentReciept.Id && x.Attendance == null).OrderBy(x => x.Id).FirstOrDefault();
                    if (attcreditFuture != null)
                    {
                        attcreditFuture.Attendance = sessionattendance;
                        Session.Flush();
                        Session.SaveOrUpdate(attcreditFuture);
                    }
                }
                sessionattendance.Invoice = newInvoiceFuture;
                Session.Flush();
                Session.SaveOrUpdate(sessionattendance);
            }
        }

        private void UpdateInvoiceDetailsWhenPaymentIsNone(Invoice invoice, Student student)
        {
            if (invoice.PaymentType == InvoicePaymentType.Daily && invoice.Status == InvoiceStatus.Pending)
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
        }

        public CreditNote CalculateCreditNoteSequenceNo(CreditNote creditNote)
        {
            var CreditNoteSequencing = Session.Query<CreditNoteSequencing>().Where(x => creditNote.CreatedDate <= x.ToDate && creditNote.CreatedDate >= x.FromDate).First();
            var creditnotes = Session.Query<CreditNote>().Where(x => x.ModifiedDate <= CreditNoteSequencing.ToDate && x.ModifiedDate >= CreditNoteSequencing.FromDate).ToList();
            int count = creditnotes.Count();
            creditNote.CreditNoteRefNumber = CreditNoteSequencing.SequenceStartNum + count + 1;
            return creditNote;
        }


        public Invoice CalculateVATAndSequenceNumber(Invoice oldinvoice)
        {
            var InvoiceSequencing = Session.Query<InvoiceSequencing>().Where(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate).First();
            var objVATDetails = Session.Query<VATDetails>().Where(x => oldinvoice.DateOfGeneration <= x.ToDate && oldinvoice.DateOfGeneration >= x.FromDate).First();
            var invoices = Session.Query<Invoice>().Where(x => x.DateOfGeneration <= InvoiceSequencing.ToDate && x.DateOfGeneration >= InvoiceSequencing.FromDate).ToList();
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

        public string GetVATCalculation(DateTime paymentDate, int paymentPlan, string discount)
        {
            if (paymentDate == null || paymentDate.Year == 1)
                return "";
            if (paymentPlan == 0)
                return "";
            decimal totalamt = Convert.ToDecimal(0);
            var paymentPlandata = Session.Query<PaymentPlan>().Where(x => x.Id == paymentPlan).First();
            if (!string.IsNullOrEmpty(discount))
                totalamt = paymentPlandata.Amount - Convert.ToDecimal(discount);
            else
                totalamt = paymentPlandata.Amount;
            var objVATDetails = Session.Query<VATDetails>().Where(x => paymentDate <= x.ToDate && paymentDate >= x.FromDate).First();

            var vatPercentage = objVATDetails?.VATPercentage ?? 20;
            decimal amtExcludingVAT = Math.Round((totalamt / (100m + vatPercentage) * 100m), 2);
            decimal vatAmout = Math.Round(totalamt - amtExcludingVAT, 2);

            //decimal finalprice =   Math.Round(((objVATDetails.VATPercentage * totalamt) / 100) / Convert.ToDecimal(1.2), 2);
            return vatAmout.ToString();
            
        }

        public InvoiceSearchResultDto SearchInvoices(SearchInvoiceDto searchInvoice)
        {
            var query = Session.QueryOver<Invoice>();


            DateTime dateFrom;
            if (DateTime.TryParse(searchInvoice.DateGeneratedFrom, out dateFrom))
            {
                query = query.Where(x => x.DateOfGeneration >= dateFrom);
            }


            DateTime dateTo;
            if (DateTime.TryParse(searchInvoice.DateGeneratedTo, out dateTo))
            {
                query = query.Where(x => x.DateOfGeneration <= dateTo);
            }

            Student student = null;

            if (!string.IsNullOrEmpty(searchInvoice.FirstName) || !string.IsNullOrEmpty(searchInvoice.LastName) || !string.IsNullOrEmpty(searchInvoice.StudentNo))
            {
                query = query.JoinAlias(x => x.Student, () => student);
            }

            if (!string.IsNullOrEmpty(searchInvoice.FirstName))
            {
                query = query.Where(() => student.FirstName == searchInvoice.FirstName);
            }

            if (!string.IsNullOrEmpty(searchInvoice.LastName))
            {
                query = query.Where(() => student.LastName == searchInvoice.LastName);
            }

            if (!string.IsNullOrEmpty(searchInvoice.StudentNo))
            {
                query = query.Where(() => student.StudentNo == searchInvoice.StudentNo);
            }

            if (searchInvoice.InvoiceStatus.HasValue)
            {
                query = query.Where(x => x.Status == searchInvoice.InvoiceStatus.Value);
            }


            IList<Invoice> list = null;

            //if page size is 0 then no pagin get everything
            if (searchInvoice.PageSize < 1)
            {
                //no pagin required get everything
                list =
                query.OrderBy(x => x.DateOfGeneration).Desc.List();
            }
            else
            {
                list = query.OrderBy(x => x.DateOfGeneration).Desc.Skip(searchInvoice.PageIndex * searchInvoice.PageSize)
            .Take(searchInvoice.PageSize).List();

            }


            var rowCount = CriteriaTransformer.TransformToRowCount(query.UnderlyingCriteria)
                .UniqueResult<int>();

            InvoiceSearchResultDto invoiceSearchResultDto = new InvoiceSearchResultDto
            {
                Data = Mapper.Map<List<InvoiceSearchResultRow>>(list),
                MaxPageIndex = rowCount > 0 && searchInvoice.PageSize >0 ? (int)Math.Ceiling((decimal)((rowCount - 1) / searchInvoice.PageSize)) : 0,
                PageSize = searchInvoice.PageSize

            };

            foreach (var data in invoiceSearchResultDto.Data)
            {
/*
                var invoicetemp = Session.Query<Invoice>().Where(x => x.Id == data.Id).First();
                data.Gross = "\u00A3" +Math.Round( (invoicetemp.TotalAfterDiscount - invoicetemp.VATAmount),2);
                data.StatusInvoice = invoicetemp.Status.ToString();
                var studenttemp = Session.Query<Student>().Where(x => x.StudentNo == data.StudentNo).First();

                data.NameOfStudent = studenttemp.FirstName + " " + studenttemp.LastName;
                data.FirstName = studenttemp.FirstName;
                data.LastName = studenttemp.LastName;
                data.Discount = invoicetemp.DiscountApplied.HasValue ? "\u00A3" + invoicetemp.DiscountApplied.Value : "\u00A3" + Convert.ToDecimal(0);
                data.NetAmount = data.TotalAfterDiscount.ToString();
                data.PaymentDate = invoicetemp.DateOfGeneration.ToString("yyyy-MM-dd");
                data.InvoiceNo = invoicetemp.InvoiceRefNumber;
                data.VATAmount = "\u00A3" + invoicetemp.VATAmount;
                */
                var creditnotes = Session.Query<CreditNote>().Where(x => x.Invoice.Id == data.Id).FirstOrDefault();
                if (creditnotes != null)
                {
                    data.CreditNotes = creditnotes.Notes;
                    data.CreditReferenceNo = creditnotes.CreditNoteRefNumber.ToString();
                }

            }
            //foreach(var data in invoiceSearchResultDto.Data)
            //{
            //    data.IsProcessed = data.
            //}

            return invoiceSearchResultDto;
        }
    }
}
