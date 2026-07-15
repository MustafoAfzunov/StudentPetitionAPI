using FluentValidation;
using StudentPetitionAPI.Domain.Enums;
using StudentPetitionAPI.Application.DTOs.Requests;

namespace StudentPetitionAPI.Application.Validators;

public class ReviewPetitionRequestValidator : AbstractValidator<ReviewPetitionRequest>
{
    public ReviewPetitionRequestValidator()
    {
        RuleFor(x => x.ReviewComment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.ReviewedBy)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .Must(status => status is PetitionStatus.Approved or PetitionStatus.Rejected)
            .WithMessage("Status must be Approved or Rejected.");
    }
}
