using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.MVC.Filters;
using AECMIS.MVC.Helpers;
using AECMIS.Service;
using AECMIS.Service.DTO;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using ClosedXML.Excel;
using System.Data;
using System.Windows.Forms;
using AECMIS.MVC.Binders;
using AECMIS.DAL.Domain;

namespace AECMIS.MVC.Controllers
{
    [AuthorizedWithSession]
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly StudentService _studentService;
        private readonly TimeTableService _timeTableService;
        private readonly TeacherService _teacherService;
        private readonly SessionRegisterService _sessionRegisterService;

        public PaymentController()
        {
            _paymentService = new PaymentService();
            _studentService = new StudentService();
            _timeTableService = new TimeTableService();
            _sessionRegisterService = new SessionRegisterService();
            _teacherService = new TeacherService();
        }

        private void BuildViewData()
        {

            ViewData["InvoiceStatuses"] = InvoiceStatus.Paid.ToSelectList();
            ViewData["AttendanceStatuses"] = SessionAttendanceStatus.Attended.ToSelectList();
            ViewData["PaymentTypes"] = PaymentType.Cash.ToSelectList();
            ViewData["PaymentPlans"] = Mapper.Map<List<PaymentPlanViewModel>>(_studentService.GetAllPaymentPlans());

        }
        //
        // GET: /Payment/
        [LoggerFilterAttribute]
        public ActionResult Index()
        {
            BuildViewData();
            var searchInvoices = new SearchInvoiceDto { PageSize = 10, PageIndex = 0 };
            InvoiceSearchResultDto result = SearchForInvoices(searchInvoices);
            result.DateGeneratedFrom = "";
            result.DateGeneratedTo = "";
            result.FirstName = "";
            result.LastName = "";
            result.StudentNo = "";
            return View(result);
        }

        private InvoiceSearchResultDto SearchForInvoices(SearchInvoiceDto searchInvoiceCriteria)
        {
            var searchResult = _paymentService.SearchInvoices(searchInvoiceCriteria);
            
            searchResult.Data.ForEach(x =>
            {
                x.ViewInvoiceUrl = Url.Action("Get", new { id = x.Id });
                x.DownloadInvoiceUrl = Url.Action("Download", new { id = x.Id });
                x.InvalidateInvoiceUrl = Url.Action("InvalidateInvoice", new { id = x.Id });
                x.UpdatePaymentUrl = Url.Action("UpdatePayment", new { id = x.Id });
                x.IsPaymentDone = x.Status.ToEnum<InvoiceStatus>() == InvoiceStatus.Paid ? true : false;
                x.currentInvoiceId = x.Id;
                x.IsProcessed = x.Status.ToEnum<InvoiceStatus>() == InvoiceStatus.Cancelled ? false : true;
                x.VerifyAdminPasswordUrl = Url.Action("VerifyAdminPassword");
                x.MakePaymentDialogDataUrl = Url.Action("MakePaymentDialogData", new { id = x.Id, isMakePayment= x.Status == "Pending" ? true : false });
            });

            
            searchResult.DoSearchUrl = Url.Action("SearchInvoices");
            // searchResult.AddRegisterUrl = Url.Action("Get");
            return searchResult;
        }

        private InvoiceSearchResultDto ExportSearchForInvoices(SearchInvoiceDto searchInvoiceCriteria)
        {
            var searchResult = _paymentService.SearchInvoices(searchInvoiceCriteria);
            
            searchResult.Data.ForEach(x =>
            {
                x.ViewInvoiceUrl = Url.Action("Get", new { id = x.Id });
                x.DownloadInvoiceUrl = Url.Action("Download", new { id = x.Id });
                x.InvalidateInvoiceUrl = Url.Action("InvalidateInvoice", new { id = x.Id });
                x.UpdatePaymentUrl = Url.Action("UpdatePayment", new { id = x.Id });
                x.IsPaymentDone = x.Status.ToEnum<InvoiceStatus>() == InvoiceStatus.Paid ? true: false;
                x.currentInvoiceId = x.Id;
                x.IsProcessed = x.Status.ToEnum<InvoiceStatus>() == InvoiceStatus.Cancelled ? false : true;
                x.VerifyAdminPasswordUrl = Url.Action("VerifyAdminPassword");
            });


            searchResult.DoSearchUrl = Url.Action("SearchInvoices");
            // searchResult.AddRegisterUrl = Url.Action("Get");
            return searchResult;
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult UpdatePayment(int invoiceId, DateTime paymentDate)
        {
            _paymentService.UpdateInvoicePaymentDate(invoiceId, paymentDate);
            return Json(true);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult VerifyAdminPassword(string password)
        {
            var adminPassword = ConfigurationManager.AppSettings["AdminPassword"];
            return Json(new { IsMatch = adminPassword == password });
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult CalculateVATData(DateTime? paymentDate, int? paymentPlan, string discount)
        {
            if (!paymentDate.HasValue)
                return Json("0.0");
            if (!paymentPlan.HasValue)
                return Json("0.0");
            var result = _paymentService.ReturnVATCalculation(paymentDate.Value, paymentPlan.Value, discount);
            return Json(result);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult InvalidateInvoice(int id)
        {
            InvalidateInvoiceViewModel invalidateInvoiceViewModel = _paymentService.InvalidateInvoice(id);
            EnableDisableControlsAtLoad(invalidateInvoiceViewModel);
            invalidateInvoiceViewModel.SaveUrl = Url.Action("SaveInvalidateInvoice", new { InvalidateInvoiceViewModel = invalidateInvoiceViewModel });
            return Json(invalidateInvoiceViewModel);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult MakePaymentDialogData(int id, bool isMakePayment=false)
        {
            InvalidateInvoiceViewModel invalidateInvoiceViewModel = _paymentService.InvalidateInvoice(id, isMakePayment);
            EnableDisableControlsAtLoad(invalidateInvoiceViewModel);
            invalidateInvoiceViewModel.SaveUrl = Url.Action("SaveInvalidateInvoice", new { InvalidateInvoiceViewModel = invalidateInvoiceViewModel });
            return Json(invalidateInvoiceViewModel);
        }

        private void EnableDisableControlsAtLoad(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            bool isInvalidateEnabled = SetPropertiesForNewInvoiceEnableDisableAtLoad(invalidateInvoiceViewModel);
            if (invalidateInvoiceViewModel.listStudentAttendanceDetails != null && invalidateInvoiceViewModel.listStudentAttendanceDetails.Count > 0 && invalidateInvoiceViewModel.listStudentAttendanceDetails[0].listFutureInvoices != null && invalidateInvoiceViewModel.listStudentAttendanceDetails[0].listFutureInvoices.Count > 1)
                invalidateInvoiceViewModel.IsFutureInvoiceAvailable = true;
            else
                invalidateInvoiceViewModel.IsFutureInvoiceAvailable = false;
            if (invalidateInvoiceViewModel.listStudentAttendanceDetails != null)
            {
                foreach (var attended in invalidateInvoiceViewModel.listStudentAttendanceDetails)
                {
                    if (invalidateInvoiceViewModel.IsFutureInvoiceAvailable)
                        attended.IsFutureInvoiceAvailable = true;
                }
            }

            invalidateInvoiceViewModel.IsInvalidateInvoiceEnabled = isInvalidateEnabled;
            if (invalidateInvoiceViewModel.listStudentAttendanceDetails?.Count == 0)
                invalidateInvoiceViewModel.IsInvalidateInvoiceEnabled = true;
        }

        private static bool SetPropertiesForNewInvoiceEnableDisableAtLoad(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            invalidateInvoiceViewModel.listStudentAttendanceDetails.ForEach(x =>
            {
                x.listAttendanceStatus = Enum.GetValues(typeof(SessionAttendanceStatus))
               .Cast<SessionAttendanceStatus>()
               .Select(t => new AttendanceType
               {
                   AttendanceId = ((int)t),
                   AttendanceStatus = t.ToString()
               }).ToList();
                x.SelectedValue = 1001;
                if (x.SelectedValue == 1002)
                {
                    x.IsNewInvoiceEnabledRow = false;
                    x.ChargedTo = "";
                }
                else if (x.PaymentTypeSelected > 0 || x.listFutureInvoices.Count > 1 || x.FutureInvoiceSelect > 0)
                {
                    if (x.PaymentTypeSelected > 0)
                        x.ChargedTo = "New Invoice";
                    x.IsNewInvoiceEnabledRow = false;
                }
                else
                {
                    x.IsNewInvoiceEnabledRow = true;
                    x.ChargedTo = "";
                }

            });
            foreach (var data in invalidateInvoiceViewModel.listStudentAttendanceDetails)
                data.IsNewInvoiceEnabledRow = true;
            bool isInvalidateEnabled = CheckIsInvalidateEnabled(invalidateInvoiceViewModel);

            return isInvalidateEnabled;
        }

        private static bool CheckIsInvalidateEnabled(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            bool isInvalidateEnabled = false;
            foreach (var attended in invalidateInvoiceViewModel.listStudentAttendanceDetails)
            {
                if ((attended.PaymentTypeSelected > 0 && !string.IsNullOrEmpty(attended.PaymentDateInStr)) || attended.FutureInvoiceSelect > 0
                    || attended.SelectedValue == 1002)
                {
                    isInvalidateEnabled = true;
                }
                else
                {
                    isInvalidateEnabled = false;
                    break;
                }
            }

            return isInvalidateEnabled;
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SaveInvalidateInvoice(InvalidateInvoiceViewModel model)
        {
            _paymentService.InvalidateInvoiceSave(model);
            return Json("true");
        }

        //int sessionAttendanceId, int PaymentType, int PaymentPlan, DateTime PaymentDate, string ChequeNo,

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SaveNewInvoice(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            InvalidateInvoiceViewModel invalidate = new InvalidateInvoiceViewModel();
            if (invalidateInvoiceViewModel.SessionAttendanceId > 0)
            {
                invalidate
                    = _paymentService.InvalidateInvoiceWithSessionAtt(invalidateInvoiceViewModel);
            }
            else
                invalidate = invalidateInvoiceViewModel;
          
            EnableDisableControlsAfterChange(invalidateInvoiceViewModel, invalidate);
            invalidate.SaveUrl = Url.Action("SaveInvalidateInvoice", new { InvalidateInvoiceViewModel = invalidate });
            return Json(invalidate);
            //return Json("true");
        }

        private void EnableDisableControlsAfterChange(InvalidateInvoiceViewModel invalidateInvoiceViewModel, InvalidateInvoiceViewModel invalidate)
        {
            var tempFutureInvoiceList = new List<FutureInvoiceAvailable>();
            var studentNo = "";
            var tempInvoiceId = 0;
            if (invalidateInvoiceViewModel.listStudentAttendanceDetails != null && invalidateInvoiceViewModel.listStudentAttendanceDetails.Count > 0)
            {
                studentNo = invalidateInvoiceViewModel.listStudentAttendanceDetails[0].StudentNo;
                tempInvoiceId = invalidateInvoiceViewModel.listStudentAttendanceDetails[0].InvoiceId;
            }
           
            if (studentNo != "" && tempInvoiceId > 0)
            {
                tempFutureInvoiceList.AddRange(_paymentService.GetFutureInvoiceAvailables(tempInvoiceId, studentNo));
            }

            GenerateTemporaryFutureInvoiceList(invalidate, tempFutureInvoiceList);
            GenerateFutureInvoiceListForNonPaymentTypeSelected(invalidate, tempFutureInvoiceList);
            SetEnableDisablePropertiesOnConditions(invalidate);
            bool isInvalidateEnabled = false;

            foreach (var attended in invalidate.listStudentAttendanceDetails)
            {
                if ((attended.PaymentTypeSelected > 0 && !string.IsNullOrEmpty(attended.PaymentDateInStr)) || attended.FutureInvoiceSelect > 0
                    || attended.SelectedValue == 1002)
                {
                    isInvalidateEnabled = true;
                }
                else
                {
                    isInvalidateEnabled = false;
                    break;
                }

            }
            if (invalidateInvoiceViewModel.listStudentAttendanceDetails != null && invalidateInvoiceViewModel.listStudentAttendanceDetails.Count > 0 && invalidateInvoiceViewModel.listStudentAttendanceDetails[0].listFutureInvoices != null && invalidateInvoiceViewModel.listStudentAttendanceDetails[0].listFutureInvoices.Count > 1)
                invalidateInvoiceViewModel.IsFutureInvoiceAvailable = true;
            else
                invalidateInvoiceViewModel.IsFutureInvoiceAvailable = false;
            invalidate.IsInvalidateInvoiceEnabled = isInvalidateEnabled;

            if (invalidateInvoiceViewModel.listStudentAttendanceDetails?.Count == 0)
                invalidateInvoiceViewModel.IsInvalidateInvoiceEnabled = true;
        }

        private static void SetEnableDisablePropertiesOnConditions(InvalidateInvoiceViewModel invalidate)
        {
            invalidate.listStudentAttendanceDetails.ForEach(x =>
            {
                x.listAttendanceStatus = Enum.GetValues(typeof(SessionAttendanceStatus))
               .Cast<SessionAttendanceStatus>()
               .Select(t => new AttendanceType
               {
                   AttendanceId = ((int)t),
                   AttendanceStatus = t.ToString()
               }).ToList();
                x.SelectedValue = x.SelectedValue == 0 ? 1001 : x.SelectedValue;
                if (x.SelectedValue == 1002)
                {
                    x.IsNewInvoiceEnabledRow = false;
                    x.ChargedTo = "";
                    x.FutureInvoiceSelect = 0;
                    x.IsFutureInvoiceAvailable = false;
                }
                else if (x.PaymentTypeSelected > 0 || x.FutureInvoiceSelect > 0)
                {
                    if (x.PaymentTypeSelected > 0)
                    {
                        x.ChargedTo = "New Invoice";
                        if (x.listFutureInvoices != null)
                            x.listFutureInvoices.Clear();
                        x.FutureInvoiceSelect = 0;
                        x.IsFutureInvoiceAvailable = false;
                    }
                    else if (x.FutureInvoiceSelect > 0)
                    {
                        x.IsFutureInvoiceAvailable = true;
                    }
                    x.IsNewInvoiceEnabledRow = false;
                }
                else
                {
                    x.IsNewInvoiceEnabledRow = true;
                    x.ChargedTo = "";
                }


                if (x.listFutureInvoices != null && x.listFutureInvoices.Count > 1 && x.FutureInvoiceSelect == 0 && x.SelectedValue != 1002)
                {
                    x.IsNewInvoiceEnabledRow = true;
                    x.IsFutureInvoiceAvailable = true;
                }


            });
        }

        private void GenerateFutureInvoiceListForNonPaymentTypeSelected(InvalidateInvoiceViewModel invalidate, List<FutureInvoiceAvailable> tempFutureInvoiceList)
        {
            foreach (var attended in invalidate.listStudentAttendanceDetails)
            {
                if (attended.listFutureInvoices != null && attended.listFutureInvoices.Count > 1 && attended.FutureInvoiceSelect < 1 && attended.PaymentTypeSelected == 0)
                {
                    if (tempFutureInvoiceList.Count < 2)
                    {
                        attended.IsFutureInvoiceAvailable = false;
                        attended.listFutureInvoices.Clear();
                        attended.listFutureInvoices.AddRange(tempFutureInvoiceList);
                    }
                    else
                    {
                        attended.listFutureInvoices.Clear();
                        attended.listFutureInvoices.AddRange(tempFutureInvoiceList);
                    }
                }
                else if (attended.listFutureInvoices != null && attended.listFutureInvoices.Count > 1 && attended.FutureInvoiceSelect > 0)
                {
                    var selectedFutureInvoice = attended.listFutureInvoices.FirstOrDefault(x => x.FIAId == attended.FutureInvoiceSelect);
                    if (selectedFutureInvoice != null)
                    {

                        var availableFutureInvoices= tempFutureInvoiceList.ToList();
                        availableFutureInvoices.Add(selectedFutureInvoice);
                        attended.listFutureInvoices.Clear();
                        attended.listFutureInvoices.AddRange(availableFutureInvoices);
                    }

                }
            }
        }

        private void GenerateTemporaryFutureInvoiceList(InvalidateInvoiceViewModel invalidate, List<FutureInvoiceAvailable> tempFutureInvoiceList)
        {
            foreach (var attended in invalidate.listStudentAttendanceDetails)
            {
                if (attended.FutureInvoiceSelect > 0)
                {
                    var selectedInvoice = tempFutureInvoiceList.FirstOrDefault(x => x.FIAId == attended.FutureInvoiceSelect);
                    if (selectedInvoice != null)
                    {
                        var filtered = tempFutureInvoiceList.Where(x => x.FIAValue != selectedInvoice.FIAValue).ToList();
                        if (filtered.Count > 1)
                        {
                            tempFutureInvoiceList.Clear();
                            tempFutureInvoiceList.AddRange(filtered);
                        }
                        else
                            tempFutureInvoiceList.Clear();
                    }
                }
            }
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult GeneratePayment(int InvoiceId, int PaymentType, int PaymentPlan, DateTime PaymentDate, string ChequeNo)
        {
            _paymentService.MakePayment(InvoiceId, PaymentType, PaymentPlan, PaymentDate, ChequeNo);   
            return Json("true");
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SearchInvoices(SearchInvoiceDto searchInvoiceCriteria)
        {
            return Json(SearchForInvoices(searchInvoiceCriteria));
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult Get(int id, bool isViewOnly = false)
        {
            return Json(_paymentService.GetWithVAT(id));
        }

        //public ActionResult ShowImage()
        //{
        //    var imagelogopath = ConfigurationManager.AppSettings["ImageLogoPath"];
        //    //Logo
        //    byte[] imgbytes = System.IO.File.ReadAllBytes(imagelogopath);
        //   return File(imgbytes , "image/jpg");
        //}

        [LoggerFilterAttribute]
        public ActionResult Download(int id)
        {
            var imagelogopath = ConfigurationManager.AppSettings["ImageLogoPath"];
            var model = _paymentService.GetWithVAT(id);
            model.LogoPath = imagelogopath;
            ViewData["VATLABEL"] = model.VATLabel;
            var html = ToHtml("PrintableInvoice", new ViewDataDictionary(model), ControllerContext);
            var fileName = BuildInvoiceFileName(model, "pdf");
           
            return BuildFileStream(html,fileName, MediaTypeNames.Application.Pdf, false);

        }

        public void BuildNewInvoiceData()
        {
            ViewData["Centers"] = _timeTableService.GetAllCentres();
            ViewData["Sessions"] = Mapper.Map<List<SessionViewModel>>(_timeTableService.GetAllSessions());
            ViewData["Teachers"] = _teacherService.GetAllTeachers();
            ViewData["AttendanceStatuses"] = SessionAttendanceStatus.Attended.ToSelectList();
            ViewData["CurriculumTypes"] = Curriculum.ALevel.ToSelectList();
            ViewData["PaymentTypes"] = PaymentType.Cash.ToSelectList();
            ViewData["RegisterStatuses"] = SessionRegisterStatus.Pending.ToSelectList();
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult GetStudentNewInvoiceDetails(int Id)
        {
            //List<StudentSubjectViewModel> viewModel = new List<StudentSubjectViewModel>();
            StudentSubjectViewModel model = _sessionRegisterService.StudentNewInvoiceDetails(Id);
            model.IsPaymentRequired = model.SessionAttendanceViewModel.PaymentRequired;
            model.AdminURL = Url.Action("VerifyAdminPassword");
            //viewModel.Add(model);
            return Json(model);
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult StudentProcessNewInvoice(int StudentId, int PaymentPlan, int PaymentType, DateTime PaymentDate, string ChequeNo,string Discount, decimal VATAmount)
        {
            _sessionRegisterService.ProcessNewInvoiceStudent(StudentId, PaymentPlan.ToString(), PaymentType.ToString(), ChequeNo, PaymentDate.ToString(), Discount, VATAmount);
            string url = Url.Action("Index");
            return Json(url);
        }

        [LoggerFilterAttribute]
        public ActionResult CreateNewInvoice()
        {
            BuildNewInvoiceData();
            
            var register = _sessionRegisterService.GetRegister(null, null);
            register.GetRegisterTemplateUrl = Url.Action("GetNewRegisterParticipants");
            register.GetStudentsStudyPlansUrl = Url.Action("GetStudentStudyPlanDetails");
            register.SaveRegisterUrl = Url.Action("StudentProcessNewInvoice");
            register.VerifyAdminPasswordUrl = Url.Action("VerifyAdminPassword");
            register.GetVATData = Url.Action("CalculateVATData");
            register.StudentSearchViewModel = new StudentSearchResultDto() { AreStudentsSelectable = true, CanAddNewStudent = false, SearchStudentsUrl = Url.Action("SearchStudent", "Student") };
            return View("CreateNewInvoice", register);
        }


        [LoggerFilterAttribute]
        public void ExportToExcel(string DateFrom, string DateTo, string StudentNo, string FirstName, string LastName, int? InvoiceStatus)
        {
            ExcelBinding(DateFrom,DateTo, StudentNo,FirstName,LastName, InvoiceStatus);
        }

        private void ExcelBinding(string DateFrom, string DateTo, string StudentNo, string FirstName, string LastName, int? InvoiceStatus)
        {
            var dateFormat = "MM/dd/yyyy";
            //if from date is not passed provided then start at beginning of this financial year

            string DateFromFormat = string.IsNullOrEmpty(DateFrom)
                ? new DateTime(DateTime.Now.Year, 4, 1).ToString(dateFormat)
                : Convert.ToDateTime(DateFrom).ToString(dateFormat);

            //if to date is not passed provided then start at beginning of this financial year
            string DateToFormat = string.IsNullOrEmpty(DateTo)
                ? new DateTime(DateTime.Now.AddYears(1).Year, 3, 31).ToString(dateFormat)
                : Convert.ToDateTime(DateTo).ToString(dateFormat);

            var searchInvoices = new SearchInvoiceDto { DateGeneratedTo = DateTo, DateGeneratedFrom = DateFrom, StudentNo = StudentNo, FirstName = FirstName, LastName = LastName, InvoiceStatus = InvoiceStatus.ToEnum<InvoiceStatus?>(null), PageSize = 0, PageIndex = 0 };

            List<InvoiceSearchResultRow> lstInvoiceSearchResultRowAll = ExportSearchForInvoices(searchInvoices).Data;
            DataTable dt = GenerateDatatableForAccountsReport(lstInvoiceSearchResultRowAll);
            DataTable dt2 = GenerateDatatableForInvoiceExcel(lstInvoiceSearchResultRowAll);

            using (XLWorkbook wb = new XLWorkbook())
            {
                int aCode = BuildFirstExcelSheet(dateFormat, DateFromFormat, DateToFormat, dt, wb);
                BuildSecondExcelSheet(dateFormat, dt2, wb, aCode);
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=AccountExport.xlsx");



                using (MemoryStream MyMemoryStream = new MemoryStream())
                {

                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }

            }
        }

        private static void BuildSecondExcelSheet(string dateFormat, DataTable dt2, XLWorkbook wb, int aCode)
        {
            #region Second Sheet
            IXLWorksheet ws1 = wb.Worksheets.Add("CreditNotes");

            IXLRange wsReportDateHeaderRange1 = ws1.Range(string.Format("A{0}:{1}{0}", 1, Char.ConvertFromUtf32(aCode + dt2.Columns.Count)));
            wsReportDateHeaderRange1.Merge();
            wsReportDateHeaderRange1.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            //wsReportDateHeaderRange1.Value = string.Format("DateFrom-DateTo :{0} - {1}", DateFromFormat, DateToFormat);


            ws1.Row(1).Style.Border.OutsideBorder = XLBorderStyleValues.None;
            ws1.Row(1).Style.Border.RightBorder = XLBorderStyleValues.None;
            ws1.Row(1).Style.Border.LeftBorder = XLBorderStyleValues.None;
            int rowIndexNew = 2;
            int columnIndexNew = 0;
            foreach (DataColumn column in dt2.Columns)
            {
                ws1.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + columnIndexNew), rowIndexNew)).Value = column.ColumnName;
                ws1.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + columnIndexNew), rowIndexNew)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                columnIndexNew++;
            }
            rowIndexNew++;
            foreach (DataRow row in dt2.Rows)
            {
                int valueCount = 0;
                foreach (object rowValue in row.ItemArray)
                {
                    ws1.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndexNew)).Value = rowValue;
                    if (valueCount == 4)
                    {
                        ws1.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndexNew)).Style.NumberFormat.SetFormat(dateFormat);
                    }
                    ws1.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndexNew)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    valueCount++;
                }
                rowIndexNew++;
            }

            ws1.Columns("1:" + (columnIndexNew - 1)).AdjustToContents();
            ws1.Columns(columnIndexNew + ":" + columnIndexNew).Width = 50;
            ws1.Range("A3:J" + rowIndexNew.ToString()).Style.Alignment.WrapText = true;
            #endregion End SecondSheet
        }

        private static int BuildFirstExcelSheet(string dateFormat, string DateFromFormat, string DateToFormat, DataTable dt, XLWorkbook wb)
        {
            int aCode = 65;
            #region FirstSheet
            IXLWorksheet ws = wb.Worksheets.Add("Invoices");

            IXLRange wsReportDateHeaderRange = ws.Range(string.Format("A{0}:{1}{0}", 1, Char.ConvertFromUtf32(aCode + dt.Columns.Count)));
            wsReportDateHeaderRange.Merge();
            wsReportDateHeaderRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            wsReportDateHeaderRange.Value = string.Format("DateFrom-DateTo :{0} - {1}", DateFromFormat, DateToFormat);


            ws.Row(2).Style.Border.OutsideBorder = XLBorderStyleValues.None;
            ws.Row(2).Style.Border.RightBorder = XLBorderStyleValues.None;
            ws.Row(2).Style.Border.LeftBorder = XLBorderStyleValues.None;
            int rowIndex = 3;
            int columnIndex = 0;
            foreach (DataColumn column in dt.Columns)
            {
                ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + columnIndex), rowIndex)).Value = column.ColumnName;
                ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + columnIndex), rowIndex)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                columnIndex++;
            }
            rowIndex++;
            foreach (DataRow row in dt.Rows)
            {
                int valueCount = 0;
                foreach (object rowValue in row.ItemArray)
                {
                    ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndex)).Value = rowValue;
                    if (valueCount == 4)
                    {
                        ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndex)).Style.NumberFormat.SetFormat(dateFormat);
                    }
                    ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndex)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    valueCount++;
                }
                rowIndex++;
            }

            ws.Columns("1:" + (columnIndex - 1)).AdjustToContents();
            ws.Columns(columnIndex + ":" + columnIndex).Width = 50;
            ws.Range("A3:J" + rowIndex.ToString()).Style.Alignment.WrapText = true;


            #endregion End FirstSheet
            return aCode;
        }

        private static DataTable GenerateDatatableForInvoiceExcel(List<InvoiceSearchResultRow> lstInvoiceSearchResultRowAll)
        {
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("Credit Note No", typeof(string));
            dt2.Columns.Add("Name Of Student", typeof(string));
            dt2.Columns.Add("Student Reference No", typeof(string));
            dt2.Columns.Add("Gross Amount", typeof(string));
            dt2.Columns.Add("VAT @ 20%", typeof(string));
            dt2.Columns.Add("Net Amount", typeof(string));
            dt2.Columns.Add("Invoice Reference No", typeof(string));
            foreach (var data in lstInvoiceSearchResultRowAll)
            {

                if (!string.IsNullOrEmpty(data.CreditReferenceNo))
                {
                    DataRow dr = dt2.NewRow();
                    dr["Invoice Reference No"] = data.InvoiceNo.ToString();
                    dr["Name of Student"] = data.Name;
                    dr["Student Reference No"] = data.StudentNo;
                    dr["Gross Amount"] = data.Gross;
                    dr["VAT @ 20%"] = data.VATAmount;
                    dr["Net Amount"] = data.NetAmount;
                    dr["Credit Note No"] = data.CreditReferenceNo;
                    dt2.Rows.Add(dr);
                }
            }

            return dt2;
        }

        private static DataTable GenerateDatatableForAccountsReport(List<InvoiceSearchResultRow> lstInvoiceSearchResultRowAll)
        {
            DataTable dt = new DataTable("accountsReport");

            dt.Columns.Add("Invoice No", typeof(string));
            dt.Columns.Add("Name Of Student", typeof(string));
            dt.Columns.Add("Student Reference No", typeof(string));
            dt.Columns.Add("Amount Paid", typeof(string));
            dt.Columns.Add("Date Paid", typeof(string));
            dt.Columns.Add("Gross Amount", typeof(string));
            dt.Columns.Add("VAT @ 20%", typeof(string));
            dt.Columns.Add("Net Amount", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("Notes", typeof(string));
            foreach (var data in lstInvoiceSearchResultRowAll)
            {
                DataRow dr = dt.NewRow();
                dr["Invoice No"] = data.InvoiceNo.ToString();
                dr["Name of Student"] = data.Name;
                dr["Student Reference No"] = data.StudentNo;
                if (string.Equals(data.StatusInvoice, "paid", StringComparison.InvariantCultureIgnoreCase))
                {
                    dr["Amount Paid"] = data.NetAmount;
                    dr["Date Paid"] = data.PaymentDate;
                }

                dr["Gross Amount"] = data.Gross;
                dr["VAT @ 20%"] = data.VATAmount;
                dr["Net Amount"] = data.NetAmount;
                dr["Status"] = data.StatusInvoice;
                dr["Notes"] = data.CreditNotes;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string BuildInvoiceFileName(InvoiceViewModel invoiceViewModel, string fileExtension)
        {
            return string.Format("Invoice_{0}_{1}.{2}", invoiceViewModel.StudentNo, invoiceViewModel.Id,fileExtension);
        }

        private FileStreamResult BuildFileStream(string fileContent, string fileName, string contentType, bool showInline)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(fileContent);

            using (var input = new MemoryStream(bytes))
            {
                var output = new MemoryStream(); // this MemoryStream is closed by FileStreamResult

                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, output);
                writer.CloseStream = false;
                document.Open();

                var xmlWorker = XMLWorkerHelper.GetInstance();
                xmlWorker.ParseXHtml(writer, document, input, System.Text.Encoding.Default);
                document.Close();
                output.Position = 0;

                var cd = new ContentDisposition
                {
                    // for example foo.bak
                    FileName = fileName,
                    
                    // always prompt the user for downloading, set to true if you want 
                    // the browser to try to show the file inline
                    Inline = showInline,
                };
                Response.AppendHeader("Content-Disposition", cd.ToString());

                //return new FileStreamResult(output, "application/pdf");
                return File(output, contentType);
            }
        }


        public string ToHtml(string viewToRender, ViewDataDictionary viewData, ControllerContext controllerContext)
        {
            var result = ViewEngines.Engines.FindView(controllerContext, viewToRender, null);

            StringWriter output;
            using (output = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, result.View, viewData, controllerContext.Controller.TempData, output);
                result.View.Render(viewContext, output);
                result.ViewEngine.ReleaseView(controllerContext, result.View);
            }

            return output.ToString();
        }
    }
}
