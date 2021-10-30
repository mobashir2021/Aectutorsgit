using System;
using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class StudentSubjectViewModel
    {
        public StudentSubjectViewModel()
        {
            SessionAttendanceViewModel = new SessionSubjectAttendanceModel();
        }


        //Subjects student is allowed to study
        public List<SubjectViewModel> Subjects { get; set; }
        
        //public Student Student { get; set; }
        public SessionSubjectAttendanceModel SessionAttendanceViewModel { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StudentNo { get; set; }
        public int StudentId { get; set; }

        //NewAddition
        public bool IsPaymentRequired { get; set; }
        public List<PaymentPlanCA> lstPaymentPlan { get; set; }
        public int PaymentPlanSelected { get; set; }
        public List<PaymentTypeCA> lstPaymentType { get; set; }
        public int PaymentTypeSelected { get; set; }
        public string ChequeNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentDateInStr { get; set; }
        public string AdminURL { get; set; }
    }
}