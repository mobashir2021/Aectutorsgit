using System;
using AECMIS.DAL.Domain;
using AECMIS.DAL.Domain.Enumerations;
using FluentValidation;
using FluentValidation.Internal;

namespace AECMIS.DAL.Validation
{
    public class SessionAttendanceValidator:AbstractValidator<SessionAttendance>
    {
        public SessionAttendanceValidator()
        {
            RuleFor(x => x.Student).NotNull();
            //RuleFor(x => x.Session).NotNull();
            //RuleFor(x => x.Date).NotEqual(DateTime.MinValue).NotEqual(DateTime.MaxValue);
            RuleFor(x => x.SubjectsAttended).NotEmpty().When(x => x.Status == SessionAttendanceStatus.Attended);
        }
        
        
    }


    public class SessionAttendanceDebitValidator : CompositeValidator<SessionAttendance>
    {
        public SessionAttendanceDebitValidator()
        {
            RegisterBaseValidator(new SessionAttendanceValidator());
            RuleFor(x => x.Status).NotNull().WithMessage("All statuses must be set for register to be processed");            
        }
    }
}
