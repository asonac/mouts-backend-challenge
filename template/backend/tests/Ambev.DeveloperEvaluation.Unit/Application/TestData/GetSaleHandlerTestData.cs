using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides methods for generating test data for GetSaleQuery and related entities.
/// This class centralizes test data generation to ensure consistency across test cases.
/// </summary>
public static class GetSaleHandlerTestData
{
    private static readonly Faker<SaleItem> saleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.ProductId, f => f.Commerce.Ean13())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 10))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(10, 200))
        .RuleFor(i => i.Discount, f => 0)
        .RuleFor(i => i.Total, (f, i) => i.UnitPrice * i.Quantity)
        .RuleFor(i => i.IsCancelled, f => false);

    private static readonly Faker<Sale> saleFaker = new Faker<Sale>()
        .RuleFor(s => s.Id, f => f.Random.Guid())
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Past(1))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, f => saleItemFaker.Generate(f.Random.Int(1, 5)))
        .RuleFor(s => s.TotalAmount, (f, s) => s.Items.Sum(i => i.Total))
        .RuleFor(s => s.IsCancelled, f => false)
        .RuleFor(s => s.CreatedAt, f => f.Date.Past(1))
        .RuleFor(s => s.UpdatedAt, f => f.Date.Recent());

    /// <summary>
    /// Generates a valid GetSaleQuery with a random sale ID.
    /// </summary>
    public static GetSaleCommand GenerateValidQuery()
    {
        var sale = saleFaker.Generate();
        return new GetSaleCommand(sale.Id);
    }

    /// <summary>
    /// Generates a GetSaleQuery with a nonexistent sale ID.
    /// Useful for testing not found scenarios.
    /// </summary>
    public static GetSaleCommand GenerateQueryWithInvalidId()
    {
        return new GetSaleCommand(Guid.NewGuid()); // Assume this ID doesn't exist
    }

    /// <summary>
    /// Generates a Sale entity with randomized data.
    /// Useful for mocking repository responses.
    /// </summary>
    public static Sale GenerateSaleEntity()
    {
        return saleFaker.Generate();
    }

    /// <summary>
    /// Generates a Sale entity with no items.
    /// Useful for edge case testing.
    /// </summary>
    public static Sale GenerateSaleWithNoItems()
    {
        var sale = saleFaker.Generate();
        sale.Items.Clear();
        sale.TotalAmount = 0;
        return sale;
    }
}
