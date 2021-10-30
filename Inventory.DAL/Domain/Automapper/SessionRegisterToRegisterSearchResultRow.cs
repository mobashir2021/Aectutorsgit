using System.Collections.Generic;
using System.Linq;
using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Domain.Extensions;
using AutoMapper;

namespace AECMIS.DAL.Domain.Automapper
{
    public class SessionRegisterToRegisterSearchResultRow : ITypeConverter<List<SessionRegister>, List<RegisterSearchResultRow>>
    {

        public List<RegisterSearchResultRow> Convert(List<SessionRegister> source, List<RegisterSearchResultRow> destination, ResolutionContext context)
        {
           return source.Select(x => new RegisterSearchResultRow
            {
                RegisterId = x.Id,
                SessionInfo = x.RegisterInfo(),
                Date = x.Date.ToString("yyyy-MM-dd"),
                Status = x.Status.ToString(),
                SessionId = x.Session.Id
            }).ToList();
        }
    }
}
