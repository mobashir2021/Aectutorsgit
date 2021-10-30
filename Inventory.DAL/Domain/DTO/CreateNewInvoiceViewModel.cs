using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Domain.DTO
{
    public class CreateNewInvoiceViewModel
    {
        public CreateNewInvoiceViewModel()
        {

        }
        public Student Student { get; set; }
        public Invoice Invoice { get; set; }
        public InvoicePayment InvoicePayment { get; set; }
        public List<PaymentPlanCA> lstPaymentPlan { get; set; }
        public int PaymentPlanSelected { get; set; }
        public List<PaymentTypeCA> lstPaymentType { get; set; }
        public int PaymentTypeSelected { get; set; }
        public string ChequeNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentDateInStr { get; set; }
        public string VerifyAdminPasswordUrl { get; set; }
        public StudentSearchResultDto StudentSearchViewModel { get; set; }
    }
}
