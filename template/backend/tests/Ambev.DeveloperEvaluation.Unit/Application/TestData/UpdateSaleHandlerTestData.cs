using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation for UpdateSaleCommand
/// to ensure consistency across test cases and provide valid and invalid scenarios.
/// </summary>
public static class UpdateSaleHandlerTestData
{
    private static readonly Faker<UpdateSaleItemDto> saleItemFaker = new Faker<UpdateSaleItemDto>()
        .RuleFor(i => i.ProductId, f => f.Commerce.Ean13())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 15))
        .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(10, 200))
        .RuleFor(i => i.IsCancelled, f => false);

    private static readonly Faker<UpdateSaleCommand> saleCommandFaker = new Faker<UpdateSaleCommand>()
        .RuleFor(s => s.Id, f => f.Random.Guid())
        .RuleFor(s => s.SaleNumber, f => f.Random.AlphaNumeric(10))
        .RuleFor(s => s.SaleDate, f => f.Date.Past(1))
        .RuleFor(s => s.CustomerId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Name.FullName())
        .RuleFor(s => s.BranchId, f => f.Random.Guid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.IsCancelled, f => false)
        .RuleFor(s => s.Items, f => saleItemFaker.Generate(f.Random.Int(1, 5)));

    /// <summary>
    /// Generates a valid UpdateSaleCommand with randomized data.
    /// </summary>
    public static UpdateSaleCommand GenerateValidCommand()
    {
        return saleCommandFaker.Generate();
    }

    /// <summary>
    /// Generates an UpdateSaleCommand with one item exceeding the quantity limit (> 20).
    /// Useful for testing business rule violations.
    /// </summary>
    public static UpdateSaleCommand GenerateCommandWithExcessiveQuantity()
    {
        var command = GenerateValidCommand();
        command.Items[0].Quantity = 25;
        return command;
    }

    /// <summary>
    /// Generates an UpdateSaleCommand with no items.
    /// Useful for testing validation failures.
    /// </summary>
    public static UpdateSaleCommand GenerateCommandWithNoItems()
    {
        var command = GenerateValidCommand();
        command.Items.Clear();
        return command;
    }

    /// <summary>
    /// Generates an UpdateSaleCommand marked as cancelled.
    /// Useful for testing event publishing logic.
    /// </summary>
    public static UpdateSaleCommand GenerateCancelledCommand()
    {
        var command = GenerateValidCommand();
        command.IsCancelled = true;
        return command;
    }

    /// <summary>
    /// Generates an UpdateSaleCommand with one item marked as cancelled.
    /// Useful for testing item-level cancellation events.
    /// </summary>
    public static UpdateSaleCommand GenerateCommandWithCancelledItem()
    {
        var command = GenerateValidCommand();
        command.Items[0].IsCancelled = true;
        return command;
    }
}
