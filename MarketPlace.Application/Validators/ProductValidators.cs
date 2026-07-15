using FluentValidation;
using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(n => n.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(n => n.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(n => n.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(n => n.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(n => n.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than zero");
    }
}
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(n => n.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters long")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(n => n.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(n => n.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(n => n.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
    }
}