using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Extensions;
using AECMIS.Service.DTO;
using AutoMapper;

namespace AECMIS.MVC.AutoMapper
{
    public class SessionToSessionViewModel : ITypeConverter<List<Session>, List<SessionViewModel>>
    {


        public List<SessionViewModel> Convert(List<Session> source, List<SessionViewModel> destination, ResolutionContext context)
        {
            return source.Select(x => new SessionViewModel
            {
                SessionId = x.Id,
                SessionDetails = x.SessionInfo(),
                Subjects = x.SubjectsTaughtAtSession.Select(s => new SubjectViewModel
                {
                    SubjectId = s.Id,
                    Name = s.Name,
                    Level = s.Level
                }),
                Location = x.Location.Id,
                Day = x.Day

            }).ToList();
        }
    }
}