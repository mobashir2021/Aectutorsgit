using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.MVC.Filters;
using AECMIS.MVC.Helpers;
using AECMIS.Service;
using AECMIS.Service.DTO;
using AutoMapper;

namespace AECMIS.MVC.Controllers
{
    [AuthorizedWithSession]
    
    public class SessionAttendanceController : Controller
    {

        private readonly SessionRegisterService _sessionRegisterService;
        private readonly TimeTableService _timeTableService;
        private readonly StudentService _studentService;

        private DateTime _attendancesFrom = DateTime.UtcNow.AddDays(-Constants.AttendancesInThePastDaysToReturn);
        private DateTime _attendancesTo = DateTime.UtcNow;

        

        public SessionAttendanceController()
        {
            _studentService = new StudentService(null);
            _sessionRegisterService = new SessionRegisterService();
            _timeTableService = new TimeTableService();
        }

        private void BuildViewData()
        {
            ViewData["CurriculumTypes"] = Curriculum.ALevel.ToSelectList();
            ViewData["AllSubjects"] = _timeTableService.GetAllSubjects().Select(x => new { x.Level, x.Id, x.Name });
            ViewData["Centers"] = _timeTableService.GetAllCentres();
            ViewData["Sessions"] = Mapper.Map<List<SessionViewModel>>(_timeTableService.GetAllSessions());
            ViewData["AttendanceStatuses"] = SessionAttendanceStatus.Attended.ToSelectList();
            ViewData["PaymentTypes"] = PaymentType.Cash.ToSelectList();
            ViewData["PaymentPlans"] = Mapper.Map<List<PaymentPlanViewModel>>(_studentService.GetAllPaymentPlans());
        }

        //
        // GET: /SessionAttendance/

        public ActionResult Index()
        {
            BuildViewData();
            

            var attendanceSearch = new SearchStudentAttendanceCriteria { PageSize = 10, PageIndex = 0, SessionsFromDate =_attendancesFrom , SessionsToDate = _attendancesTo};
            
            return View(SearchForAttendances(attendanceSearch));
        }

        private SearchAttendanceResult SearchForAttendances(SearchStudentAttendanceCriteria searchCriteria)
        {
            var searchResult = _sessionRegisterService.GetAttendances(searchCriteria);
            searchResult.Attendances.ForEach(x =>
            {
                x.EditUrlDetails = Url.Action("GetAttendanceData", new { id = x.SessionAttendanceId });
                x.VerifyAdminPasswordUrl = Url.Action("VerifyAdminPassword");
                x.listAttendanceStatus = Enum.GetValues(typeof(SessionAttendanceStatus))
               .Cast<SessionAttendanceStatus>()
               .Select(t => new AttendanceType
               {
                   AttendanceId = ((int)t),
                   AttendanceStatus = t.ToString()
               }).ToList();

            });
            searchResult.SearchAttendancesUrl = Url.Action("SearchAttendances");
            searchResult.AttendancesFrom = _attendancesFrom.Date.ToString("dd/MM/yyyy");
            searchResult.AttendancesTo = _attendancesTo.Date.ToString("dd/MM/yyyy");
            return searchResult;
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult VerifyAdminPassword(string password)
        {
            var adminPassword = ConfigurationManager.AppSettings["AdminPassword"];
            return Json(new { IsMatch = adminPassword == password });
        }

        [LoggerFilterAttribute]
        public JsonResult GetAttendanceData(int id)
        {
            var searchResult = _sessionRegisterService.GetStudentAttendanceDetails(id);
            var status = Enum.GetValues(typeof(SessionAttendanceStatus))
               .Cast<SessionAttendanceStatus>()
               .Select(t => new AttendanceType
               {
                   AttendanceId = ((int)t),
                   AttendanceStatus = t.ToString()
               }).ToList();
            searchResult.listAttendanceStatus = status.Where(x => x.AttendanceStatus != searchResult.Status).OrderBy(x => x.AttendanceId).ToList();
            if (searchResult.RemainingCredits == 0)
            {
                if ((searchResult.listAttendanceStatus[0].AttendanceStatus == "AuthorizeAbsence"))
                {
                    searchResult.IsNewInvoiceEnabled = false;
                    searchResult.IsSaveEnabled = true;
                    searchResult.IsFutureInvoiceAvailable = false;
                    searchResult.ChargedTo = "";
                }
                else if (((searchResult.listAttendanceStatus[0].AttendanceStatus == "Attended" 
                    || searchResult.listAttendanceStatus[0].AttendanceStatus == "UnAuthorizedAbsence") && searchResult.RemainingCredits < 1)
                    && searchResult.Status == "AuthorizeAbsence")
                {
                    searchResult.IsNewInvoiceEnabled = true;
                    searchResult.IsSaveEnabled = false;
                    
                }
                else
                {
                    searchResult.IsNewInvoiceEnabled = false;
                    searchResult.IsSaveEnabled = true;
                }
            }
            else
            {
                searchResult.IsNewInvoiceEnabled = false;
                searchResult.IsSaveEnabled = true;
                searchResult.IsFutureInvoiceAvailable = false;
            }

            return Json(searchResult);
        }

        [LoggerFilterAttribute]
        public JsonResult GetAttendanceInvoice(int invoiceid)
        {
            var searchResultList = _sessionRegisterService.GetStudentAttendanceInvoices(invoiceid);
            var status = Enum.GetValues(typeof(SessionAttendanceStatus))
               .Cast<SessionAttendanceStatus>()
               .Select(t => new AttendanceType
               {
                   AttendanceId = ((int)t),
                   AttendanceStatus = t.ToString()
               }).ToList();
            foreach (var searchResult in searchResultList)
            {
                searchResult.listAttendanceStatus = status.Where(x => x.AttendanceStatus != searchResult.Status).ToList();
            }
            return Json(searchResultList);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult InvalidateInvoice(int SessionAttendanceId, int InvoiceId, string Notes)
        {
            _sessionRegisterService.ChangeAttendanceStatusChargeToNonCharge(SessionAttendanceId, InvoiceId, Notes);
            bool result = true;
            return Json(result);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult NonChargebleToChargable(int SessionAttendanceId, int? NewStatus, int? InvoiceId, string StudentNo, string PaymentType, string PaymentPlan,
            string ChequeNo, string PaymentDate, string IsDailyInvoiceData, string FutureInvoiceSelect)
        {
            int FutureInvoiceSelectData = 0;
            if (!string.IsNullOrEmpty(FutureInvoiceSelect))
                FutureInvoiceSelectData = int.Parse(FutureInvoiceSelect);
            int InvoiceIdValue = 0;
            if (InvoiceId.HasValue)
                InvoiceIdValue = InvoiceId.Value;
            _sessionRegisterService.ChangeAttendanceStatusNonChargeableToChargeble(SessionAttendanceId, InvoiceIdValue, NewStatus.HasValue ? NewStatus.Value : 0, StudentNo, PaymentPlan, PaymentType, ChequeNo, PaymentDate, IsDailyInvoiceData, FutureInvoiceSelectData);
            bool result = true;
            return Json(result);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult ChargebleToNonChargable(int SessionAttendanceId)
        {
            _sessionRegisterService.ChangeAttendanceStatusChargebleToNonChargable(SessionAttendanceId);
            bool result = true;
            return Json(result);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult ChargebleToChargable(int SessionAttendanceId, int NewStatus)
        {
            _sessionRegisterService.ChargeToChargable(SessionAttendanceId, NewStatus);
            bool result = true;
            return Json(result);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult CreateNewInvoice(string StudentNo, string PaymentType, string PaymentPlan,
            string ChequeNo, int SessionAttendanceId, string PaymentDate, string IsDailyInvoiceData)
        {
            int invoiceid = _sessionRegisterService.ProcessNewInvoice(StudentNo, SessionAttendanceId, PaymentPlan, PaymentType, ChequeNo, PaymentDate, IsDailyInvoiceData);
            return Json(invoiceid, JsonRequestBehavior.AllowGet);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SearchAttendances(SearchStudentAttendanceCriteria searchCriteria )
        {
            return Json(SearchForAttendances(searchCriteria));
        }

    }
}
