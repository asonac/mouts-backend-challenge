using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation for CreateSaleCommand
/// to ensure consistency across test cases and provide valid scenarios.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<SaleItemDto> saleItemFaker = new Faker<SaleItemDto>()
        .RuleFor(i => i.ProductId, f => f.Commerce.Ean13())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 15))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(10, 200));

    private static readonly Faker<CreateSaleCommand> saleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Past(1))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, f => saleItemFaker.Generate(f.Random.Int(1, 5)));

    /// <summary>
    /// Generates a valid CreateSaleCommand with randomized data.
    /// </summary>
    public static CreateSaleCommand GenerateValidCommand()
    {
        return saleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates a CreateSaleCommand with one item exceeding the quantity limit (> 20).
    /// Useful for testing business rule violations.
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithExcessiveQuantity()
    {
        var command = GenerateValidCommand();
        command.Items[0].Quantity = 25;
        return command;
    }

    /// <summary>
    /// Generates a CreateSaleCommand with no items.
    /// Useful for testing validation failures.
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithNoItems()
    {
        var command = GenerateValidCommand();
        command.Items.Clear();
        return command;
    }
}
