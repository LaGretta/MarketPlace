using FluentValidation;
using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Validators;

public class RegisterDtoValid : AbstractValidator<RegisterDto>
{
    public RegisterDtoValid()
    {
        RuleFor(n => n.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters");
        RuleFor(n => n.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(150).WithMessage("Password cannot exceed 150 characters");
        RuleFor(n => n.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not Valid");
    }
}
public class LoginDtoValid : AbstractValidator<LoginDto>
{
    public LoginDtoValid()
    {
        RuleFor(n => n.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");
        
        RuleFor(n => n.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(3).WithMessage("Password must be at least 3 characters long")
            .MaximumLength(150).WithMessage("Password cannot exceed 150 characters");
    }
}