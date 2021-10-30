using AECMIS.DAL.Domain;
using FluentValidation;

namespace AECMIS.DAL.Validation
{
    public class TuitionCentreValidator : AbstractValidator<TuitionCentre>
    {
        public TuitionCentreValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("A centre name must be provided");
            RuleFor(x => x.Address).NotEmpty().WithMessage("A centre address must be provided");
        }
    }
}
