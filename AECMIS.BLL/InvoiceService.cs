using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using AECMIS.DAL.Nhibernate.Repositories;
using System.Linq;
namespace AECMIS.Service
{
    public class InvoiceService
    {
        private Repository<Invoice, int> _invoiceRepository;

        public InvoiceService()
        {
            _invoiceRepository = new Repository<Invoice, int>();
        }       

    }
}
