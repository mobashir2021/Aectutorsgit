using System;
using System.Collections.Generic;

namespace AECMIS.DAL.Domain.DTO
{
    public class StudentAttendanceDetails
    {
        public string StudentName { get; set; }
        public string StudentNo { get; set; }
        public string Teacher { get; set; }
        public string Session { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        //Changes added -- Removed column
        //public int RemainingCredits { get; set; }
        public bool IsProcessed { get; set; }
        public List<StudentPaymentDetails> PaymentDetails { get; set; }
        public string EditUrlDetails { get; set; }
        public int SessionAttendanceId { get; set; }
        public string SaveUrl { get; set; }
        public List<AttendanceType> listAttendanceStatus { get; set; }
        public string ChargedTo { get; set; }
        public int RemainingCredits { get; set; }
        public string RemainingCreditInStr { get; set; }
        public string IsDailyInvoiceData { get; set; }
        public int NewStatus { get; set; }
        public int InvoiceId { get; set; }
        public bool IsSaveEnabled { get; set; }
        public bool IsNewInvoiceEnabled { get; set; }
        public int SelectedValue { get; set; }
        public List<FutureInvoiceAvailable> listFutureInvoices { get; set; }
        public int FutureInvoiceSelect { get; set; }
        public List<PaymentPlanCA> lstPaymentPlan { get; set; }
        public int PaymentPlanSelected { get; set; }
        public List<PaymentTypeCA> lstPaymentType { get; set; }
        public int PaymentTypeSelected { get; set; }
        public string ChequeNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentDateInStr { get; set; }
        public bool IsCreateNewInvoiceEnableInvalidate { get; set; }
        public string VerifyAdminPasswordUrl { get; set; }
        public bool IsNewInvoiceEnabledRow { get; set; }

        public bool IsFutureInvoiceAvailable { get; set; }

    }

    public class AttendanceType
    {
        public int AttendanceId { get; set; }
        public string AttendanceStatus { get; set; }
    }

    public class PaymentPlanCA
    {
        public int PaymentPlanId { get; set; }
        public string PaymentPlanValue { get; set; }
    }

    public class PaymentTypeCA
    {
        public int PaymentTypeId { get; set; }
        public string PaymentTypeDesc { get; set; }
    }

    public class FutureInvoiceAvailable
    {
        public int FIAId { get; set; }
        public string FIAValue { get; set; }
    }

    public class StudentPaymentDetails
    {
        public string InvoiceType { get; set; }
        public string PaymentType { get; set; }
        public string PaymentAmount { get; set; }
        public string InvoiceNo { get; set; }
    }

    public class SearchStudentAttendanceCriteria
    {
        public string StudentNo { get; set; }
        public DateTime? Dob { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Subject { get; set; }
        public int? Curriculum { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public DateTime? SessionsFromDate { get; set; }
        public DateTime? SessionsToDate { get; set; }
    }

    public class SearchAttendanceResult
    {
        public List<StudentAttendanceDetails> Attendances { get; set; }
        public string SearchAttendancesUrl { get; set; }
        public int MaxPageIndex { get; set; }
        public int PageSize { get; set; }
        public string AttendancesFrom { get; set; }
        public string AttendancesTo { get; set; }
    }

}
