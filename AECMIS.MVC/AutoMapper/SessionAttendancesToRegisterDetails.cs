using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Extensions;
using AECMIS.MVC.Helpers;
using AECMIS.Service.DTO;
using AutoMapper;
using System.Linq;
namespace AECMIS.MVC.AutoMapper
{
    public class SessionAttendancesToRegisterDetails : ITypeConverter<List<SessionAttendance>, List<RegisterDetailViewModel>>
    {

        public List<RegisterDetailViewModel> Convert(List<SessionAttendance> source, List<RegisterDetailViewModel> destination, ResolutionContext context)
        {
            return source.Distinct(new SessionAttendanceComparer()).
                Select(x => new RegisterDetailViewModel
                {
                    SessionId = x.SessionRegister.Session.Id,
                    RegisterDateTime = x.SessionRegister.Date,
                    RegisterDetails = x.SessionRegister.RegisterInfo(),

                }).ToList();
        }

        internal class SessionAttendanceComparer : IEqualityComparer<SessionAttendance>
        {
            public bool Equals(SessionAttendance x, SessionAttendance y)
            {
                if (x == null || y == null)
                    return false;

                return x.SessionRegister.Date.CompareTo(y.SessionRegister.Date) == 0 &&
                       x.SessionRegister.Session.Id == y.SessionRegister.Session.Id;
            }

            public int GetHashCode(SessionAttendance obj)
            {
                if (obj == null)
                    return 0;

                return obj.GetHashCode();
            }

        }
    }
}