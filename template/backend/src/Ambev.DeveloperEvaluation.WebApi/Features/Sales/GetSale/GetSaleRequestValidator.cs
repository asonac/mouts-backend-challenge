using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>
/// Validator for CreateSaleRequest used in the API layer.
/// </summary>
public class GetSaleRequestValidator : AbstractValidator<GetSaleRequest>
{
    public GetSaleRequestValidator()
    {
        RuleFor(sale => sale.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
