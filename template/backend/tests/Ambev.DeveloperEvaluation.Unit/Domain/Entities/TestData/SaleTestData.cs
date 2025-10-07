using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sale entities using the Bogus library.
/// Centralizes valid and invalid data generation for consistent and reusable test scenarios.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.ProductId, f => f.Commerce.Ean13())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 15))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(5, 100))
        .RuleFor(i => i.IsCancelled, f => false);

    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Past(1))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, f => SaleItemFaker.Generate(f.Random.Int(1, 5)))
        .RuleFor(s => s.IsCancelled, f => false)
        .RuleFor(s => s.CreatedAt, f => DateTime.UtcNow);

    /// <summary>
    /// Generates a valid Sale entity with randomized data.
    /// </summary>
    public static Sale GenerateValidSale()
    {
        var sale = SaleFaker.Generate();
        sale.ApplyDiscountsAndCalculateTotal();
        return sale;
    }

    /// <summary>
    /// Generates a Sale entity with empty or invalid fields for negative testing.
    /// </summary>
    public static Sale GenerateInvalidSale()
    {
        return new Sale
        {
            SaleNumber = "", // Invalid
            SaleDate = DateTime.MinValue, // Invalid
            CustomerId = "", // Invalid
            CustomerName = "", // Invalid
            BranchId = "", // Invalid
            BranchName = "", // Invalid
            Items = new List<SaleItem>() // Invalid: empty
        };
    }

    /// <summary>
    /// Generates a SaleItem with quantity above the allowed limit (e.g. > 20).
    /// </summary>
    public static SaleItem GenerateItemWithExcessiveQuantity()
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = 25; // Invalid: exceeds business rule
        return item;
    }

    /// <summary>
    /// Generates a SaleItem marked as cancelled.
    /// </summary>
    public static SaleItem GenerateCancelledItem()
    {
        var item = SaleItemFaker.Generate();
        item.IsCancelled = true;
        return item;
    }

    /// <summary>
    /// Generates a Sale with at least one cancelled item.
    /// </summary>
    public static Sale GenerateSaleWithCancelledItem()
    {
        var sale = GenerateValidSale();
        sale.Items.ToList()[0].IsCancelled = true;
        return sale;
    }

    /// <summary>
    /// Generates a Sale marked as cancelled.
    /// </summary>
    public static Sale GenerateCancelledSale()
    {
        var sale = GenerateValidSale();
        sale.Cancel();
        return sale;
    }
}
