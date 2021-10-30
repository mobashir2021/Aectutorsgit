using System;
using AECMIS.DAL.Validation;
using FluentValidation.Results;

namespace AECMIS.DAL.Domain
{
   public class TuitionCentre:Entity
    {
       public virtual string Name { get; set; }
       public virtual string Address { get; set; }

       public virtual ValidationResult IsValid()
       {
            return new TuitionCentreValidator().Validate(this);
       }
    }
}
