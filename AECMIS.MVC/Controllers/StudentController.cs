using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using AECMIS.MVC.AutoMapper;
using AECMIS.MVC.Binders;
using AECMIS.MVC.Filters;
using AECMIS.MVC.Helpers;
using AECMIS.MVC.Models;
using AECMIS.Service;
using AECMIS.Service.DTO;
using AutoMapper;
using System.Configuration;
using ClosedXML.Excel;
using System.Data;
using System.IO;

namespace AECMIS.MVC.Controllers
{
    [AuthorizedWithSession]
    public class StudentController : Controller
    {

        private readonly StudentService _studentService;
        private readonly SessionRegisterService _timeTableService;

        private string _imageStoragePath;

        public StudentController()
        {
            _studentService = new StudentService(null);
            _timeTableService = new SessionRegisterService();
            _imageStoragePath = ConfigurationManager.AppSettings["StudentImagesRelativePath"];
        }

        //private List<PaymentPlanViewModel> GetAllPaymentPlans()
        //{
        //    //Mapper.Map<List<PaymentPlanViewModel>>(_studentService.GetAllPaymentPlans());
        //    return new List<PaymentPlanViewModel>()
        //                                   {

        //                                       new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 1,
        //                                               PaymentPlanDisplay = "5 Sessions-(Gcse - £80.00)",
        //                                               Curriculum = Curriculum.Gcse,
        //                                               TotalNumberOfSessions = 5
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 2,
        //                                               PaymentPlanDisplay = "10 Sessions-(Gcse - £130.00)",
        //                                               Curriculum = Curriculum.Gcse,
        //                                               TotalNumberOfSessions = 10
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 3,
        //                                               PaymentPlanDisplay = "20 Sessions-(Gcse - £180.00)",
        //                                               Curriculum = Curriculum.Gcse,
        //                                               TotalNumberOfSessions = 20
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 4,
        //                                               PaymentPlanDisplay = "5 Sessions-(KeyStage1 - £50.00)",
        //                                               Curriculum = Curriculum.KeyStage1,
        //                                               TotalNumberOfSessions = 5
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 5,
        //                                               PaymentPlanDisplay = "10 Sessions-(KeyStage1 - £110.00)",
        //                                               Curriculum = Curriculum.KeyStage1,
        //                                               TotalNumberOfSessions = 10
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 6,
        //                                               PaymentPlanDisplay = "20 Sessions-(KeyStage1 - £150.00)",
        //                                               Curriculum = Curriculum.KeyStage1,
        //                                               TotalNumberOfSessions = 20
        //                                           },
        //                                           new PaymentPlanViewModel
        //                                           {
        //                                               PaymentPlanId = 7,
        //                                               PaymentPlanDisplay = "2 Sessions-(GCSE - £26.00)",
        //                                               Curriculum = Curriculum.Gcse,
        //                                               TotalNumberOfSessions = 2
        //                                           }

        //                                   };
        //}
        //private List<SessionViewModel> GetAllSessions()
        //{
        //    const string sessionStr = "Session #{0} on {1}@{2} - {3}"; //, DayOfWeek.Monday,
        //    //x.From.HoursAndMins(), x.To.HoursAndMins());
        //    //Mapper.Map<List<SessionViewModel>>(_sessionService.GetAllSessions());
        //    return new List<SessionViewModel>
        //               {
        //                   new SessionViewModel
        //                       {
        //                           SessionId = 1,
        //                           SessionDetails = string.Format(sessionStr,"00001",DayOfWeek.Monday,"11:00","13:00"),
        //                           Location = 1,
        //                           Subjects = new List<SubjectViewModel>
        //                                          {
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Maths",SubjectId = 1},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Science",SubjectId = 2},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="English",SubjectId = 3},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Maths",SubjectId = 4},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Science",SubjectId = 5},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="English",SubjectId = 6},
        //                                              new SubjectViewModel{Level = Curriculum.ALevel,Name="Physics",SubjectId = 7},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Maths",SubjectId = 8},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Science",SubjectId = 9},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="English",SubjectId = 10},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Maths",SubjectId = 11},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Science",SubjectId = 12},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="English",SubjectId = 13}

        //                                          }

        //                       },

        //                       new SessionViewModel
        //                       {
        //                           SessionId = 2,
        //                           SessionDetails = string.Format(sessionStr,"00002",DayOfWeek.Monday,"13:00","15:00"),
        //                           Location = 1,
        //                           Subjects = new List<SubjectViewModel>
        //                                          {                                                      
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="English",SubjectId = 3},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Maths",SubjectId = 4},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Science",SubjectId = 5},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="English",SubjectId = 6},
        //                                              new SubjectViewModel{Level = Curriculum.ALevel,Name="Physics",SubjectId = 7},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Maths",SubjectId = 8},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Science",SubjectId = 9},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="English",SubjectId = 10},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Maths",SubjectId = 11},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Science",SubjectId = 12},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="English",SubjectId = 13}
        //                                          }
        //                       },
        //                       new SessionViewModel
        //                       {
        //                           SessionId = 3,
        //                           SessionDetails = string.Format(sessionStr,"00003",DayOfWeek.Sunday,"11:00","13:00"),
        //                           Location = 1,
        //                           Subjects = new List<SubjectViewModel>
        //                                          {                                                      
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="English",SubjectId = 3},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Maths",SubjectId = 4},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Science",SubjectId = 5},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="English",SubjectId = 6},
        //                                              new SubjectViewModel{Level = Curriculum.ALevel,Name="Physics",SubjectId = 7},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Maths",SubjectId = 8},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Science",SubjectId = 9},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="English",SubjectId = 10},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Maths",SubjectId = 11},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Science",SubjectId = 12},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="English",SubjectId = 13}
        //                                          }
        //                       },

        //                       new SessionViewModel
        //                       {
        //                           SessionId = 4,
        //                           SessionDetails = string.Format(sessionStr,"00004",DayOfWeek.Sunday,"13:00","15:00"),
        //                           Location = 1,
        //                           Subjects = new List<SubjectViewModel>
        //                                          {
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Maths",SubjectId = 1},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Science",SubjectId = 2},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="English",SubjectId = 3},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Maths",SubjectId = 4},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="Science",SubjectId = 5},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage1,Name="English",SubjectId = 6},
        //                                              new SubjectViewModel{Level = Curriculum.ALevel,Name="Physics",SubjectId = 7},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Maths",SubjectId = 8},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="Science",SubjectId = 9},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage2,Name="English",SubjectId = 10},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Maths",SubjectId = 11},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="Science",SubjectId = 12},
        //                                              new SubjectViewModel{Level = Curriculum.KeyStage3,Name="English",SubjectId = 13}
        //                                          }

        //                       },
        //                       new SessionViewModel
        //                       {
        //                           SessionId = 5,
        //                           SessionDetails = string.Format(sessionStr,"00005",DayOfWeek.Tuesday,"13:00","15:00"),
        //                           Location = 1,
        //                           Subjects = new List<SubjectViewModel>
        //                                          {
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Maths",SubjectId = 1},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="Science",SubjectId = 2},
        //                                              new SubjectViewModel{Level = Curriculum.Gcse,Name="English",SubjectId = 3},

        //                                          }

        //                       }
        //               };
        //}

        //private IEnumerable<StudentSearchResultRow> GetStudentSearchResults()
        //{
        //    var results = new List<StudentSearchResultRow>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        var curriculum = Curriculum.KeyStage1;
        //        if(i > 49)
        //            curriculum = Curriculum.KeyStage2;

        //        results.Add(new StudentSearchResultRow
        //                        {
        //                            FirstName = "Dummy"+i,
        //                            LastName = "Dummy"+i,
        //                            StudentId = i,
        //                            Curriculum = curriculum.ToString(),
        //                            StudentNo = "0000"+1
        //                        });
        //    }

        //    return results;
        //}

        //private StudentSearchResultDto SearchDummyStudent(StudentSearchDto searchDto)
        //{
        //    var query = from s in GetStudentSearchResults() select s;

        //    if (!string.IsNullOrEmpty(searchDto.FirstName))
        //        query = query.Where(x => x.FirstName == searchDto.FirstName);
        //    if (!string.IsNullOrEmpty(searchDto.LastName))
        //        query = query.Where(x => x.LastName == searchDto.LastName);
        //    if (!string.IsNullOrEmpty(searchDto.StudentNo))
        //        query = query.Where(x => x.StudentNo == searchDto.StudentNo);
        //    if (searchDto.Curriculum !=null)
        //        query = query.Where(x => x.Curriculum == searchDto.Curriculum.GetValueOrDefault().ToString());

        //    IList<StudentSearchResultRow> studentsSearchResult =
        //        query.OrderBy(x => x.StudentNo).ToList();

        //    var list = studentsSearchResult.Skip(searchDto.PageIndex * searchDto.PageSize)
        //            .Take(searchDto.PageSize).ToList();

        //    var rowCount = studentsSearchResult.Count();

        //    return new StudentSearchResultDto
        //    {
        //        Students = Mapper.Map<List<StudentSearchResultRow>>(list),
        //        MaxPageIndex = (int)Math.Ceiling((decimal)((rowCount - 1) / searchDto.PageSize)),
        //        PageSize = searchDto.PageSize
        //    };

        //}

        private void BuildViewData()
        {
            ViewData["Genders"] = Gender.Male.ToSelectList();
            ViewData["RelationTypes"] = RelationType.Father.ToSelectList();
            ViewData["CurriculumTypes"] = Curriculum.ALevel.ToSelectList();
            ViewData["PaymentPlans"] = Mapper.Map<List<PaymentPlanViewModel>>(_studentService.GetAllPaymentPlans());
            //ViewData["Sessions"] = Mapper.Map<List<SessionViewModel>>(_sessionService.GetAllSessions());
            ViewData["InstituteTypes"] = InstituteType.College.ToSelectList();
            ViewData["TitleTypes"] = TitleTypes.Miss.ToSelectList();
        }

        //
        // GET: /Student/
        [LoggerFilterAttribute]
        public ActionResult Index()
        {
            BuildViewData();
            var studentSearch = new StudentSearchDto { PageSize = 10, PageIndex = 0, ActiveOnly = true };
            return View("StudentSearch", SearchStudents(studentSearch));
        }


        [LoggerFilterAttribute]
        [HttpPost]
        public JsonResult SearchStudent(StudentSearchDto studentSearch)
        {
            return Json(SearchStudents(studentSearch));
        }

        private StudentSearchResultDto SearchStudents(StudentSearchDto studentSearch)
        {
            var searchResults = _studentService.SearchStudents(studentSearch);
            //var searchResults = SearchDummyStudent(studentSearch);
            searchResults.Students.ForEach(x =>
                                               {
                                                   x.EditUrl = Url.Action("Get", new { Id = x.StudentId });
                                                   x.DeleteUrl = Url.Action("Delete", new { Id = x.StudentId });

                                               });
            searchResults.SearchStudentsUrl = Url.Action("SearchStudent");
            searchResults.AddStudentUrl = Url.Action("Get");
            return searchResults;
        }

        [LoggerFilterAttribute]
        public ActionResult Get(int? id)
        {
            BuildViewData();
            return View("StudentDetails", GetStudentViewModel(id));
        }

        [LoggerFilterAttribute]
        public ActionResult Delete(int id)
        {
            _studentService.DeleteStudent(id);
            return RedirectToAction("Index");
        }


        private StudentDetailsDto GetStudentViewModel(int? studentId)
        {
            var model = studentId != null
                       ? Mapper.Map<StudentDetailsDto>(_studentService.Get(studentId.GetValueOrDefault()))
                       : new StudentDetailsDto(Constants.MinAge, Constants.MaxAge, StudentToStudentDetailsDto.MapSessions(null));

            model.SearchStudentsUrl = Url.Action("Index");
            return model;
        }

        [LoggerFilterAttribute]
        [HttpPost]
        public ActionResult SaveStudent([FromJson]StudentDetailsDto model)
        {
            var path = Server.MapPath(_imageStoragePath);
            _studentService.Save(model, path);
            return RedirectToAction("Index");
        }


        [LoggerFilterAttribute]
        public void ExportStudentsWithNoCredits(string firstName, string lastName, string studentNo,  int? curriculum, bool? activeOnly)
        {

            var students = _studentService.GetStudentsWithNoCredits(new StudentSearchDto() { FirstName = firstName, LastName = lastName, Curriculum= (Curriculum?)curriculum,  StudentNo= studentNo, ActiveOnly = activeOnly.GetValueOrDefault() }); 
            DataTable dt = new DataTable("contacts");

            dt.Columns.Add("SRN", typeof(string));
            dt.Columns.Add("SFN", typeof(string));
            dt.Columns.Add("SLN", typeof(string));
            dt.Columns.Add("S.No", typeof(string));
            dt.Columns.Add("Due", typeof(string));
            foreach (var data in students)
            {
                foreach (var contact in data.Contacts.Select(x => x.ContactPhone))
                {
                    DataRow dr = dt.NewRow();
                    dr["SRN"] = data.StudentNo;
                    dr["SFN"] = data.FirstName;
                    dr["SLN"] = data.LastName;
                    dr["S.No"] = contact.MobileNumber;
                    dr["Due"] = data.DefaultPaymentPlan != null ? data.DefaultPaymentPlan.Amount - data.DiscountAmount : 0;
                    dt.Rows.Add(dr);
                }
            }

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=PaymentsDue.xlsx");

            using (XLWorkbook wb = new XLWorkbook())
            {
                var aCode = 65;
                var ws = wb.Worksheets.Add("Contacts");


                ws.Row(1).Style.Border.OutsideBorder = XLBorderStyleValues.None;
                ws.Row(1).Style.Border.RightBorder = XLBorderStyleValues.None;
                ws.Row(1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                int rowIndex = 1;
                int columnIndex = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + columnIndex), rowIndex)).Value = column.ColumnName;
                    columnIndex++;
                }
                rowIndex++;
                foreach (DataRow row in dt.Rows)
                {
                    int valueCount = 0;
                    foreach (object rowValue in row.ItemArray)
                    {
                        ws.Cell(string.Format("{0}{1}", Char.ConvertFromUtf32(aCode + valueCount), rowIndex)).Value = rowValue;
                        valueCount++;
                    }
                    rowIndex++;
                }

                ws.Columns("1:" + (columnIndex - 1)).AdjustToContents();
                ws.Columns(columnIndex + ":" + columnIndex).Width = 50;
                ws.Range("A3:J" + rowIndex.ToString()).Style.Alignment.WrapText = true;

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }
    }
}
