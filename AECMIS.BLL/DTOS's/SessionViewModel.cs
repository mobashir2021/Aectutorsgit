using System;
using System.Collections.Generic;
using AECMIS.DAL.Domain.DTO;

namespace AECMIS.Service.DTO
{
    public class SessionViewModel
    {
        public string SessionDetails { get; set; }
        public int SessionId { get; set; }
        public int Location { get; set; }
        public IEnumerable<SubjectViewModel> Subjects { get; set; }
        public DayOfWeek Day { get; set; }
        public bool IsSelected { get; set; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
    }
}