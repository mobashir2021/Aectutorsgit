using AECMIS.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECMIS.DAL.Nhibernate.Mappings
{
    public class VATDetailsMap : BaseMap<VATDetails>
    {
        public VATDetailsMap()
        {
            Table("payment.VATDetails");
            Id(x => x.Id).Column("Id").GeneratedBy.Identity();
            Map(x => x.FiscalYear).Column("FiscalYear").Not.Nullable();
            Map(x => x.FromDate).Column("FromDate").Not.Nullable();
            Map(x => x.ToDate).Column("ToDate").Not.Nullable();
            Map(x => x.VATPercentage).Column("VATPercentage").Not.Nullable();
        }
    }
}
