using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for SaleItem entities using the Bogus library.
/// Centralizes valid and invalid data generation for consistent and reusable test scenarios.
/// </summary>
public static class SaleItemTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.ProductId, f => f.Commerce.Ean13())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 15))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(5, 100))
        .RuleFor(i => i.IsCancelled, f => false);

    /// <summary>
    /// Generates a valid SaleItem with randomized data.
    /// </summary>
    public static SaleItem GenerateValidItem()
    {
        var item = SaleItemFaker.Generate();
        item.ApplyDiscount();
        return item;
    }

    /// <summary>
    /// Generates a SaleItem with excessive quantity (e.g. > 20) to test business rule violations.
    /// </summary>
    public static SaleItem GenerateItemWithExcessiveQuantity()
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = 25;
        return item;
    }

    /// <summary>
    /// Generates a SaleItem with zero quantity to test validation failures.
    /// </summary>
    public static SaleItem GenerateItemWithZeroQuantity()
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = 0;
        return item;
    }

    /// <summary>
    /// Generates a SaleItem marked as cancelled.
    /// </summary>
    public static SaleItem GenerateCancelledItem()
    {
        var item = SaleItemFaker.Generate();
        item.IsCancelled = true;
        item.ApplyDiscount();
        return item;
    }

    /// <summary>
    /// Generates a SaleItem with negative unit price to test edge cases.
    /// </summary>
    public static SaleItem GenerateItemWithNegativePrice()
    {
        var item = SaleItemFaker.Generate();
        item.UnitPrice = -10m;
        return item;
    }

    /// <summary>
    /// Generates a SaleItem with empty product name and ID to test validation errors.
    /// </summary>
    public static SaleItem GenerateItemWithMissingProductInfo()
    {
        var item = SaleItemFaker.Generate();
        item.ProductId = "";
        item.ProductName = "";
        item.ApplyDiscount();
        return item;
    }
}
