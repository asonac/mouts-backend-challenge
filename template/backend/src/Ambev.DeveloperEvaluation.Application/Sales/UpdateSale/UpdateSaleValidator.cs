using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand that defines validation rules for sale creation.
/// </summary>
/// <remarks>
/// Validation rules include:
/// - SaleNumber: Required, 3–20 characters
/// - SaleDate: Cannot be in the future
/// - CustomerId and BranchId: Required
/// - CustomerName and BranchName: Required, max 100 characters
/// - Items: Must contain at least one item, each validated by SaleItemValidator
/// </remarks>
public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateSaleCommandValidator with defined validation rules.
    /// </summary>
    public UpdateSaleCommandValidator()
    {
        RuleFor(sale => sale.Id)
           .NotEmpty().WithMessage("Id is required.");

        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required.")
            .Length(3, 20).WithMessage("Sale number must be between 3 and 20 characters.");

        RuleFor(sale => sale.SaleDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Sale date cannot be in the future.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(100).WithMessage("Branch name cannot exceed 100 characters.");

        RuleFor(sale => sale.Items)
            .NotEmpty().WithMessage("Sale must contain at least one item.")
            .ForEach(item => item.SetValidator(new UpdateSaleItemDtoValidator()));
    }
}
