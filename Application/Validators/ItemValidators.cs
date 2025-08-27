using Application.DTOs;
using FluentValidation;

namespace Application.Validators
{
    public class ItemCreateValidator : AbstractValidator<ItemCreateDto>
    {
        public ItemCreateValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}
