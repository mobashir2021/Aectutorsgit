using AECMIS.DAL.Domain.DTO;
using AECMIS.DAL.Nhibernate.Repositories;
using AutoMapper;
using System;
using System.Collections.Generic;

namespace AECMIS.Service
{
    public class PaymentService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public PaymentService()
        {
            _invoiceRepository = new InvoiceRepository();
        }

        public InvoiceSearchResultDto SearchInvoices(SearchInvoiceDto searchInvoice)
        {
            return _invoiceRepository.SearchInvoices(searchInvoice);
        }

        public InvoiceViewModel Get(int invoiceId)
        {
            var invoice = _invoiceRepository.Get(invoiceId);
            return Mapper.Map<InvoiceViewModel>(invoice);
        }

        public string ReturnVATCalculation(DateTime paymentDate, int paymentPlan, string discount)
        {
            return _invoiceRepository.GetVATCalculation(paymentDate, paymentPlan, discount);
        }

        public InvoiceViewModel GetWithVAT(int invoiceId)
        {
            var invoice = _invoiceRepository.Get(invoiceId);
            return _invoiceRepository.GetWithVAT(Mapper.Map<InvoiceViewModel>(invoice));
        }

        public void UpdateInvoicePaymentDate(int invoiceId, DateTime paymentDate)
        {
            var invoice = _invoiceRepository.Get(invoiceId);
            if (invoice.PaymentReciept != null)
            {
                invoice.PaymentReciept.GeneratedDate = paymentDate;
                invoice.PaymentReciept.InvoicePayment.PaymentDate = paymentDate;
                _invoiceRepository.Save(invoice);
            }

        }

        public void InvalidateInvoiceSave(InvalidateInvoiceViewModel invalidateInvoice)
        {
            _invoiceRepository.SaveInvalidateInvoice(invalidateInvoice);
        }

        public CreateNewInvoiceViewModel GetNewInvoiceViewModel()
        {
            return _invoiceRepository.GetNewInvoiceViewModel();
        }
        
        public void MakePayment(int InvoiceId, int PaymentType, int PaymentPlan, DateTime PaymentDate, string ChequeNo)
        {
            _invoiceRepository.MakePaymentForInvoice(InvoiceId, PaymentType, PaymentPlan, PaymentDate, ChequeNo);
        }

        public InvalidateInvoiceViewModel InvalidateInvoice(int invoiceId, bool isMakePayment = false)
        {
            var invoice = _invoiceRepository.Get(invoiceId);
            InvalidateInvoiceViewModel invalidateInvoiceViewModel = new InvalidateInvoiceViewModel()
            {
                Id = invoice.Id,
                listStudentAttendanceDetails = _invoiceRepository.GetSessionAttendancesForGivenInvoice(invoiceId, isMakePayment)
            };
            return invalidateInvoiceViewModel;
        }

        public InvalidateInvoiceViewModel InvalidateInvoiceWithSessionAtt(InvalidateInvoiceViewModel invalidateInvoiceViewModel)
        {
            //var invoice = _invoiceRepository.Get(SessionAttendanceId);
            InvalidateInvoiceViewModel invalidateInvoiceViewModelreturn = new InvalidateInvoiceViewModel()
            {
                listStudentAttendanceDetails = _invoiceRepository.GetSessionAttendancesWithInvoice(invalidateInvoiceViewModel),
                CreditNote = invalidateInvoiceViewModel.CreditNote
            };
            invalidateInvoiceViewModelreturn.Id = invalidateInvoiceViewModel.listStudentAttendanceDetails[0].InvoiceId;
            return invalidateInvoiceViewModelreturn;
        }

        public List<FutureInvoiceAvailable> GetFutureInvoiceAvailables(int invoiceid, string studentno)
        {
            return _invoiceRepository.GetAvailableFutureInvoices(invoiceid, studentno);
        }
    }
    
}
