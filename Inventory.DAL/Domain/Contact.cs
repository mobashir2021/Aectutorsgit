using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AECMIS.DAL.Domain.Contracts;

namespace AECMIS.DAL.Domain
{
   public class Contact:Entity, IContact
    {
       public virtual string HomeNumber
       {
           get;
           set;
       }

       public virtual string WorkNumber
       {
           get;
           set;
       }

       public virtual string MobileNumber
       {
           get;
           set;
       }

       public virtual string Email
       {
           get;
           set;
       }

       public virtual ContactPerson ContactPerson { get; set; }
    }
}
