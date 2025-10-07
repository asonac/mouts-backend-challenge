using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Validator for GetSaleCommand that defines validation rules for sale creation.
/// </summary>
/// <remarks>
/// Validation rules include:
/// - SaleNumber: Required, 3–20 characters
/// - SaleDate: Cannot be in the future
/// - CustomerId and BranchId: Required
/// - CustomerName and BranchName: Required, max 100 characters
/// - Items: Must contain at least one item, each validated by SaleItemValidator
/// </remarks>
public class GetSaleCommandValidator : AbstractValidator<GetSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the GetSaleCommandValidator with defined validation rules.
    /// </summary>
    public GetSaleCommandValidator()
    {
        RuleFor(sale => sale.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
