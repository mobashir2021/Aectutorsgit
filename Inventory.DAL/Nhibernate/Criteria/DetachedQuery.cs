using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace AECMIS.DAL.Nhibernate.Criteria
{
    public class DetachedQuery
    {
        public static void GetDebitableInvoices(int studentId, ISession session)
        {
            //SessionAttendance s = null;
            //Invoice i = null;
            //var subqueryquery = QueryOver.Of<PaymentReciept>().
            //    Inner.JoinAlias(x => x.Invoice, () => i).
            //    Left.JoinAlias(t => t.PayedAttendences, () => s).
            //    Where(x => i.Student.Id == studentId).
            //    SelectList(list=> list.
            //        SelectGroup(b=> i.Id).
            //        SelectGroup(b=> b.Id).
            //        SelectCount(b=> s.Id));

            //SessionAttendance s = null;
            //Invoice i = null;
            //PaymentReciept p = null;
            //const string subqueryAlias = "";
            //var subquery = QueryOver.Of(() => s).Where(() => s.PaymentReciept.Id == p.Id).ToRowCountQuery();

            //var query = QueryOver.Of(() => p).
            //    Inner.JoinAlias(x => x.Invoice, () => i).
            //    Where(() => i.Student.Id == studentId)
            //    .SelectList(list => list.
            //                            Select(b => b.Id).
            //                            Select(b => b.Invoice.Id).
            //                            SelectSubQuery(subquery));
            ////return query.DetachedCriteria.GetExecutableCriteria(session).List<PaymentReciept>();
            //return query;
        }
    }
}
