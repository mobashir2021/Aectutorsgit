using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Mvc;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.MVC.Binders;
using AECMIS.MVC.Filters;
using AECMIS.MVC.Helpers;
using AECMIS.MVC.Models;
using AECMIS.Service.DTO;
using AECMIS.Service;
using AutoMapper;

namespace AECMIS.MVC.Controllers
{

    public enum CommandType
    {
        Process = 1001,
        Save = 1000
    }

    [AuthorizedWithSession]
    public class SessionRegisterController : Controller
    {
        private readonly SessionRegisterService _sessionRegisterService;
        private readonly TimeTableService _timeTableService;
        private readonly TeacherService _teacherService;
 
        public SessionRegisterController()
        {
            _sessionRegisterService = new SessionRegisterService();
            _teacherService = new TeacherService();
            _timeTableService = new TimeTableService();
        }

        private void BuildViewData()
        {
            ViewData["Centers"] = _timeTableService.GetAllCentres();
            ViewData["Sessions"] = Mapper.Map<List<SessionViewModel>>(_timeTableService.GetAllSessions());
            ViewData["Teachers"] = _teacherService.GetAllTeachers();
            ViewData["AttendanceStatuses"] = SessionAttendanceStatus.Attended.ToSelectList();
            ViewData["CurriculumTypes"] = Curriculum.ALevel.ToSelectList();
            ViewData["PaymentTypes"] = PaymentType.Cash.ToSelectList();
            ViewData["RegisterStatuses"] = SessionRegisterStatus.Pending.ToSelectList();
        }

        //
        // GET: /SessionRegister/
        [LoggerFilterAttribute]
        public ActionResult Index()
        {
            BuildViewData();
            var registerSearch = new SearchRegisterDto {PageSize = 10,PageIndex = 0};
            return View("SessionRegisterSearch", SearchForRegisters(registerSearch));
        }

        [LoggerFilterAttribute]
        public ActionResult Get(int? id, DateTime? date)
        {
            BuildViewData();
            var register = _sessionRegisterService.GetRegister(id, date);
            register.GetRegisterTemplateUrl = Url.Action("GetNewRegisterParticipants");
            register.GetStudentsStudyPlansUrl = Url.Action("GetStudentStudyPlanDetails");
            register.SaveRegisterUrl = Url.Action("SaveRegister");
            register.VerifyAdminPasswordUrl = Url.Action("VerifyAdminPassword");
            register.StudentSearchViewModel = new StudentSearchResultDto() { AreStudentsSelectable = true,CanAddNewStudent = false,SearchStudentsUrl = Url.Action("SearchStudent","Student")};
            return View("SessionRegister", register);
        }

        [LoggerFilterAttribute]
        public ActionResult GetReadonlyView(int id, DateTime date)
        {
            BuildViewData();
            var register = _sessionRegisterService.GetViewOnlyRegister(id, date);
            register.StudentSearchViewModel = new StudentSearchResultDto() { AreStudentsSelectable = true, CanAddNewStudent = false, SearchStudentsUrl = Url.Action("SearchStudent", "Student") };
            return View("SessionRegister", register);
        }

        private RegisterSearchResultDto SearchForRegisters(SearchRegisterDto searchRegisterCriteria)
        {
            var searchResult = _sessionRegisterService.SearchRegisters(searchRegisterCriteria);
            searchResult.Registers.ForEach(x=>
                                               {
                                                   x.EditUrl = Url.Action("Get", new {id=x.SessionId,date=x.Date});                                                   

                                               });

            searchResult.SearchRegistersUrl = Url.Action("SearchStudentRegisters");
            searchResult.AddRegisterUrl = Url.Action("Get");
            return searchResult;
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult GetNewRegisterParticipants(int sessionId)
        {
            return Json(_sessionRegisterService.GetRegister(sessionId, null));
        }

        [HttpPost]
        public JsonResult GetStudentStudyPlanDetails(List<int> studentIds, int sessionId)
        {
            return Json(_sessionRegisterService.GetStudyPlanDetailsByStudentId(studentIds, sessionId, null));
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult VerifyAdminPassword(string password)
        {
           var adminPassword =  ConfigurationManager.AppSettings["AdminPassword"];
            return Json(new {IsMatch=adminPassword == password});        
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SearchStudentRegisters(SearchRegisterDto searchRegisterCriteria)
        {
            return Json(SearchForRegisters(searchRegisterCriteria));
        }

    
        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SaveRegister([FromJson]SessionRegisterViewModel model, CommandType commandType)
        {
            var result = true;
            var error = "";
            try
            {
                if (commandType == CommandType.Process)
                {
                    _sessionRegisterService.ProcessRegister(model, 0);
                }
                else
                {
                    _sessionRegisterService.SaveSessionAttendances(model, 0);
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);
                result = false;
                error = e.Message;
            }


            //throw new NotImplementedException();
            //return Get(model.SessionId, Convert.ToDateTime(model.Date));
            return Json(BuildResponse(result,error,commandType, model.SessionId,Convert.ToDateTime(model.Date)));
        }

        private static string CommandString(CommandType currentCommand)
        {
            return currentCommand == CommandType.Process ? "Processed" : "Saved";
        }

        private object BuildResponse(bool result, string error, CommandType currentCommand, int sessionId, DateTime date)
        {
            var message = "Session Register";

            message += !result
                           ? string.Format(" could not be {0} error thrown : {1}", CommandString(currentCommand), error)
                           : " " + CommandString(currentCommand);


            return new {success=  result, message, actions = BuildNavigationOptions(currentCommand,sessionId,date)};
        }

        private List<NavigationItem> BuildNavigationOptions(CommandType currentCommand, int sessionId, DateTime date)
        {
            var list = new List<NavigationItem>();
            var strDate = date.ToString("yyyy-MM-dd");
            var text = string.Empty;
            if (currentCommand == CommandType.Save)
            {
                text = "Process Register";
                list.Add(new NavigationItem { Text = text, Url = Url.Action("Get", new { id = sessionId, date = strDate }) });
            }
            else
            {
                text = "View Register";
                list.Add(new NavigationItem { Text = text, Url = Url.Action("GetReadonlyView", new { id = sessionId, date = strDate }) });                
            }            
            
            list.Add(new NavigationItem { Text = "Add New Register", Url = Url.Action("Get") });
            list.Add(new NavigationItem { Text = "Find Existing Register", Url = Url.Action("Index") });

            return list;
        }
    }
}
