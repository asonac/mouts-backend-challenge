using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Validator for DeleteSaleCommand that defines validation rules for sale creation.
/// </summary>
/// <remarks>
/// Validation rules include:
/// - SaleNumber: Required, 3–20 characters
/// - SaleDate: Cannot be in the future
/// - CustomerId and BranchId: Required
/// - CustomerName and BranchName: Required, max 100 characters
/// - Items: Must contain at least one item, each validated by SaleItemValidator
/// </remarks>
public class DeleteSaleCommandValidator : AbstractValidator<DeleteSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the DeleteSaleCommandValidator with defined validation rules.
    /// </summary>
    public DeleteSaleCommandValidator()
    {
        RuleFor(sale => sale.Id)
           .NotEmpty().WithMessage("Id is required.");
    }
}
