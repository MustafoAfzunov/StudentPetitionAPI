using FluentValidation;
using StudentPetitionAPI.Application.DTOs.Requests;

namespace StudentPetitionAPI.Application.Validators;

public class ListPetitionsRequestValidator : AbstractValidator<ListPetitionsRequest>
{
    public ListPetitionsRequestValidator()
    {
        // Page/PageSize are normalized in the service (same behavior as before).
        RuleFor(x => x)
            .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo)
            .WithMessage("'dateFrom' cannot be later than 'dateTo'.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.PetitionType)
            .IsInEnum()
            .When(x => x.PetitionType.HasValue);
    }
}
