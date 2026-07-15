using FluentValidation;
using StudentPetitionAPI.Application.DTOs.Requests;

namespace StudentPetitionAPI.Application.Validators;

public class CreatePetitionRequestValidator : AbstractValidator<CreatePetitionRequest>
{
    public CreatePetitionRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.PetitionType)
            .IsInEnum()
            .WithMessage("PetitionType is required and must be a valid value.");
    }
}
