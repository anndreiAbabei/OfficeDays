using System.Linq.Expressions;
using FluentValidation;

namespace OfficeDays.Features.Vacations;

public abstract class VacationDatesValidator<T> : AbstractValidator<T>
{
    protected VacationDatesValidator(Expression<Func<T, DateOnly>> from, Expression<Func<T, DateOnly>> to)
    {
        RuleFor(to).GreaterThanOrEqualTo(from);
    }
}
