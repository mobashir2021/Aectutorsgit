using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.Contracts;

namespace AECMIS.DAL.Domain
{
    public class Address :Entity, IAddress
    {

        public virtual string AddressLine1
        {
            get;
            set;
        }

        public virtual string AddressLine2
        {
            get;
            set;
        }

        public virtual string AddressLine3
        {
            get;
            set;
        }

        public virtual string AddressLine4
        {
            get;
            set;
        }

        public virtual string AddressLine5
        {
            get;
            set;
        }

        public virtual string PostCode
        {
            get;
            set;
        }

        public virtual string County
        {
            get;
            set;
        }

        public virtual string City
        {
            get;
            set;
        }

    }
}
