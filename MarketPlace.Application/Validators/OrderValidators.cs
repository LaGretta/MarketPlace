using FluentValidation;
using MarketPlace.Application.DTOs;

namespace MarketPlace.Application.Validators;

public class CreateOrderDtoValid : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValid()
    {
        RuleFor(n => n.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(n => n.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("ProductId must be greater than zero");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });
    }
}
