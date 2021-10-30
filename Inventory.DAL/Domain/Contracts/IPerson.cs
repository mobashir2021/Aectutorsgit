using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain.Contracts
{
    public interface IPerson
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string MiddleName { get; set; }
        int? Age { get; set; }
    }
}
