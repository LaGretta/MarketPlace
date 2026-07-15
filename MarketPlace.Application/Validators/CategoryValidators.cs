using FluentValidation;
using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Validators;

public class CreateCategoryDtoValid : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValid()
    {
        RuleFor(n => n.Name)
            .NotEmpty().WithMessage("Name must not be empty")
            .MinimumLength(3).WithMessage("Name must contain 3 characters")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");

    }
}
public class UpdateCategoryDtoValid : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValid()
    {
        RuleFor(n => n.Name)
            .NotEmpty().WithMessage("Name must not be empty")
            .MinimumLength(3).WithMessage("Name must contain 3 characters")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}