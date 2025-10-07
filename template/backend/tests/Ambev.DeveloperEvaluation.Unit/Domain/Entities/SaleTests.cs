using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the Sale entity class.
/// Tests cover validation, discount application, and cancellation logic.
/// </summary>
public class SaleTests
{
    /// <summary>
    /// Tests that validation passes when all sale properties are valid.
    /// </summary>
    [Fact(DisplayName = "Validation should pass for valid sale data")]
    public void Given_ValidSaleData_When_Validated_Then_ShouldReturnValid()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var result = sale.Validate();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validation fails when sale properties are invalid.
    /// </summary>
    [Fact(DisplayName = "Validation should fail for invalid sale data")]
    public void Given_InvalidSaleData_When_Validated_Then_ShouldReturnInvalid()
    {
        // Arrange
        var sale = new Sale
        {
            SaleNumber = "", // Invalid
            SaleDate = DateTime.MinValue, // Invalid
            CustomerId = "", // Invalid
            CustomerName = "", // Invalid
            BranchId = "", // Invalid
            BranchName = "", // Invalid
            Items = new List<SaleItem>() // Invalid: empty
        };

        // Act
        var result = sale.Validate();

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    /// <summary>
    /// Tests that discounts are applied correctly and total is calculated.
    /// </summary>
    [Fact(DisplayName = "Should apply discounts and calculate total amount")]
    public void Given_SaleWithItems_When_ApplyDiscountsAndCalculateTotal_Then_TotalShouldBeCorrect()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.ApplyDiscountsAndCalculateTotal();

        // Assert
        Assert.True(sale.TotalAmount > 0);
        Assert.All(sale.Items, item => Assert.True(item.Total > 0));
    }

    /// <summary>
    /// Tests that cancelling a sale sets IsCancelled to true and updates the timestamp.
    /// </summary>
    [Fact(DisplayName = "Sale should be marked as cancelled when Cancel is called")]
    public void Given_Sale_When_Cancelled_Then_IsCancelledShouldBeTrue()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.Cancel();

        // Assert
        Assert.True(sale.IsCancelled);
        Assert.NotNull(sale.UpdatedAt);
    }
}
