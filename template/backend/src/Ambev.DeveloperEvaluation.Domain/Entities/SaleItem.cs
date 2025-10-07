using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents an item within a sale transaction.
/// Includes quantity-based discount logic and total calculation.
/// </summary>
public class SaleItem
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }

    /// <summary>
    /// Applies discount based on quantity tiers.
    /// </summary>
    public void ApplyDiscount()
    {
        if (Quantity > 20)
            throw new InvalidOperationException($"Cannot sell more than 20 units of product {ProductName}");

        if (Quantity >= 10)
            Discount = UnitPrice * Quantity * 0.20m;
        else if (Quantity >= 4)
            Discount = UnitPrice * Quantity * 0.10m;
        else
            Discount = 0;

        Total = (UnitPrice * Quantity) - Discount;
    }

    /// <summary>
    /// Cancels the item.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// Validates the item using SaleItemValidator.
    /// </summary>
    public ValidationResultDetail Validate()
    {
        var validator = new SaleItemValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(e => (ValidationErrorDetail)e)
        };
    }
}
