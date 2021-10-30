using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain.DTO
{
    public class SearchSessionDto
    {
        public int? sessionId { get; set; }
        public DayOfWeek? dayOfWeek { get; set; }
        public int? centreId { get; set; }
    }
}
