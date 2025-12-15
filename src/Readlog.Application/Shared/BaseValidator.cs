using FluentValidation;

namespace Readlog.Application.Shared;

public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected BaseValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}
