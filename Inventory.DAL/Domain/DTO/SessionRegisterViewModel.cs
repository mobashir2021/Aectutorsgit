using System;
using System.Collections.Generic;
using System.Data;
using AECMIS.DAL.Domain.Enumerations;

namespace AECMIS.DAL.Domain.DTO
{
    public class SessionRegisterViewModel
    {
        public SessionRegisterViewModel()
        {
            SessionAttendees = new List<StudentSubjectViewModel>();
            ExistingRegisters = new List<DateTime>();
        }

        public int? SessionRegisterId { get; set; }
        public string Date { get; set; }
        public int SessionId { get; set; }
        public List<StudentSubjectViewModel> SessionAttendees { get; set; }
        public List<DateTime> ExistingRegisters { get; set; }
        public string GetRegisterTemplateUrl { get; set; }
        public string GetStudentsStudyPlansUrl { get; set; }
        public string VerifyAdminPasswordUrl { get; set; }
        public string GetVATData { get; set; }
        public string SaveRegisterUrl { get; set; }
        public StudentSearchResultDto StudentSearchViewModel { get; set; }
        public int? Center { get; set; }
        public bool RegisterDetailsAreReadOnly { get; set; }
        public bool RegisterLocationDetailsAreReadOnly { get; set; }
    }
}