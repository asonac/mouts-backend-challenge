using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

/// <summary>
/// Contains unit tests for the SaleItem entity.
/// Covers discount logic, total calculation, cancellation, and validation.
/// </summary>
public class SaleItemTests
{
    [Fact(DisplayName = "Should apply 20% discount when quantity >= 10")]
    public void Given_QuantityGreaterThanOrEqualTo10_When_ApplyDiscount_Then_DiscountShouldBe20Percent()
    {
        var item = SaleItemTestData.GenerateValidItem();
        item.Quantity = 10;
        item.UnitPrice = 100;

        item.ApplyDiscount();

        Assert.Equal(200m, item.Discount);
        Assert.Equal(800m, item.Total);
    }

    [Fact(DisplayName = "Should apply 10% discount when quantity between 4 and 9")]
    public void Given_QuantityBetween4And9_When_ApplyDiscount_Then_DiscountShouldBe10Percent()
    {
        var item = SaleItemTestData.GenerateValidItem();
        item.Quantity = 5;
        item.UnitPrice = 50;

        item.ApplyDiscount();

        Assert.Equal(25m, item.Discount);
        Assert.Equal(225m, item.Total);
    }

    [Fact(DisplayName = "Should apply no discount when quantity < 4")]
    public void Given_QuantityLessThan4_When_ApplyDiscount_Then_DiscountShouldBeZero()
    {
        var item = SaleItemTestData.GenerateValidItem();
        item.Quantity = 2;
        item.UnitPrice = 30;

        item.ApplyDiscount();

        Assert.Equal(0m, item.Discount);
        Assert.Equal(60m, item.Total);
    }

    [Fact(DisplayName = "Should throw exception when quantity > 20")]
    public void Given_QuantityGreaterThan20_When_ApplyDiscount_Then_ShouldThrowException()
    {
        var item = SaleItemTestData.GenerateItemWithExcessiveQuantity();

        var ex = Assert.Throws<InvalidOperationException>(() => item.ApplyDiscount());
        Assert.Contains("Cannot sell more than 20 units", ex.Message);
    }

    [Fact(DisplayName = "Should mark item as cancelled")]
    public void Given_Item_When_Cancelled_Then_IsCancelledShouldBeTrue()
    {
        var item = SaleItemTestData.GenerateValidItem();

        item.Cancel();

        Assert.True(item.IsCancelled);
    }

    [Fact(DisplayName = "Validation should pass for valid item")]
    public void Given_ValidItem_When_Validated_Then_ShouldReturnValid()
    {
        var item = SaleItemTestData.GenerateValidItem();

        var result = item.Validate();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validation should fail for zero quantity")]
    public void Given_ZeroQuantity_When_Validated_Then_ShouldReturnInvalid()
    {
        var item = SaleItemTestData.GenerateItemWithZeroQuantity();

        var result = item.Validate();

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Validation should fail for missing product info")]
    public void Given_MissingProductInfo_When_Validated_Then_ShouldReturnInvalid()
    {
        var item = SaleItemTestData.GenerateItemWithMissingProductInfo();

        var result = item.Validate();

        Assert.False(result.IsValid);
    }
}
