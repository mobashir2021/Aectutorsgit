using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AECMIS.DAL.Domain.Contracts
{
    public interface IContact
    {
        string HomeNumber { get; set; }
        string WorkNumber { get; set; }
        string MobileNumber { get; set; }
        string Email { get; set; }
    }
}
