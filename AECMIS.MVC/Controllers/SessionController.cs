using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AECMIS.DAL.Domain;
using AECMIS.MVC.Filters;
using AECMIS.Service.DTO;
using AutoMapper;

namespace AECMIS.MVC.Controllers
{
    [AuthorizedWithSession]
    public class SessionController : Controller
    {
        //
        // GET: /Session/

        private IEnumerable<TuitionCentre> GetCentres()
        {
            return new List<TuitionCentre>()
                       {
                           new TuitionCentre
                               {
                                   Name = "Walthamstow",
                                   Address = "Hoe St, London",
                                   Id = 1

                               },
                           new TuitionCentre
                               {
                                   Name = "East Ham",
                                   Address = "High St, London",
                                   Id = 2
                               }
                       };
        }
        //private IEnumerable<SessionAttendance> GetAttendances()
        //{
        //    return new List<SessionAttendance>()
        //               {
        //                new SessionAttendance
        //                    {
        //                        Id = 1,
        //                        Date = new DateTime(2013,5,15,11,00,00),
        //                        Session = new Session{Day = DayOfWeek.Monday,From = new TimeSpan(11,00,00),To = new TimeSpan(13,00,00),Id = 1},
        //                    } ,
        //                    new SessionAttendance
        //                    {
        //                        Id = 2,
        //                        Date = new DateTime(2013,5,15,11,00,00),
        //                        Session = new Session{Day = DayOfWeek.Monday,From = new TimeSpan(11,00,00),To = new TimeSpan(13,00,00),Id = 1},
        //                    },
        //                    new SessionAttendance
        //                    {
        //                        Id = 3,
        //                        Date = new DateTime(2013,5,16,11,00,00),
        //                        Session = new Session{Day = DayOfWeek.Tuesday,From = new TimeSpan(11,00,00),To = new TimeSpan(13,00,00),Id = 2},
                                
        //                    },
        //                     new SessionAttendance
        //                    {
        //                        Id = 3,
        //                        Date = new DateTime(2013,5,17,11,00,00),
        //                        Session = new Session{Day = DayOfWeek.Wednesday,From = new TimeSpan(11,00,00),To = new TimeSpan(13,00,00),Id = 3},
        //                    }
        //               };
        //}

        //private List<Session> GetAllSessions()
        //{
        //    return new List<Session>()
        //               {
        //                   new Session
        //                       {
        //                           Day = DayOfWeek.Monday,
        //                           From = new TimeSpan( 11, 00, 00),
        //                           To = new TimeSpan(13, 00, 00),
        //                           Id = 1,
        //                           Location = new TuitionCentre{Id = 1}
        //                       },
        //                   new Session
        //                       {
        //                           Day = DayOfWeek.Tuesday,
        //                           From = new TimeSpan(11, 00, 00),
        //                           To = new TimeSpan(13, 00, 00),
        //                           Id = 2,
        //                           Location = new TuitionCentre{Id = 2}
        //                       },
        //                   new Session
        //                       {
        //                           Day = DayOfWeek.Wednesday,
        //                           From = new TimeSpan(11, 00, 00),
        //                           To = new TimeSpan(13, 00, 00),
        //                           Id = 3,
        //                           Location = new TuitionCentre{Id = 1}
        //                       },
        //                       new Session
        //                       {
        //                           Day = DayOfWeek.Wednesday,
        //                           From = new TimeSpan( 11, 00, 00),
        //                           To = new TimeSpan( 13, 00, 00),
        //                           Id = 4,
        //                           Location = new TuitionCentre{Id = 2}
        //                       }
        //               };
        //}

        //private void BuildViewData()
        //{
        //    ViewData["AllCentres"] = GetCentres();
        //    ViewData["AllSessions"] = Mapper.Map<List<SessionViewModel>>(GetAllSessions());
        //}

        //public ActionResult Index()
        //{
        //    return View();
        //}

    }
}
