using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _handler;
    private readonly CreateSaleCommandValidator _validator = new();

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _eventPublisher);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale { Id = Guid.NewGuid(), Items = new List<SaleItem>() };
        var result = new CreateSaleResult { Id = sale.Id };

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(sale.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // Empty command

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Should pass validation for valid sale command")]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Should fail when SaleNumber is missing or too short")]
    public void Validate_MissingOrShortSaleNumber_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.SaleNumber = "";

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SaleNumber");
    }

    [Fact(DisplayName = "Should fail when SaleDate is in the future")]
    public void Validate_FutureSaleDate_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.SaleDate = DateTime.UtcNow.AddDays(1);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SaleDate");
    }

    [Fact(DisplayName = "Should fail when CustomerId is missing")]
    public void Validate_MissingCustomerId_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.CustomerId = "";

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact(DisplayName = "Should fail when BranchName exceeds 100 characters")]
    public void Validate_LongBranchName_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.BranchName = new string('A', 101);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BranchName");
    }

    [Fact(DisplayName = "Should fail when no items are provided")]
    public void Validate_EmptyItems_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithNoItems();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact(DisplayName = "Should fail when item quantity exceeds 20")]
    public void Validate_ItemQuantityExceedsLimit_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateCommandWithExcessiveQuantity();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName.Contains("Items[0].Quantity") &&
            e.ErrorMessage.Contains("Cannot sell more than 20 identical items"));
    }

    [Fact(DisplayName = "Should fail when item has negative price")]
    public void Validate_ItemWithNegativePrice_ShouldFail()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items[0].UnitPrice = -5;

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Items[0].UnitPrice"));
    }




    [Fact(DisplayName = "Given valid sale When creating Then publishes SaleCreatedEvent")]
    public async Task Handle_ValidRequest_PublishesSaleCreatedEvent()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale { Id = Guid.NewGuid(), Items = new List<SaleItem>() };

        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult { Id = sale.Id });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _eventPublisher.Received(1).PublishAsync(Arg.Is<SaleCreatedEvent>(e => e.Sale.Id == sale.Id), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid command When handling Then maps command to sale entity")]
    public async Task Handle_ValidRequest_MapsCommandToSale()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale { Id = Guid.NewGuid(), Items = new List<SaleItem>() };

        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult { Id = sale.Id });

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<Sale>(Arg.Is<CreateSaleCommand>(c =>
            c.SaleNumber == command.SaleNumber &&
            c.CustomerId == command.CustomerId &&
            c.BranchId == command.BranchId &&
            c.Items.Count == command.Items.Count));
    }

    [Fact(DisplayName = "Given item with quantity between 4 and 9 When creating sale Then applies 10% discount")]
    public async Task Handle_QuantityBetween4And9_Applies10PercentDiscount()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = new List<SaleItemDto>
        {
            new SaleItemDto
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Product B",
                Quantity = 5,
                UnitPrice = 50
            }
        };

        var sale = new Sale
            {
                Id = Guid.NewGuid(),
                Items = new List<SaleItem>
            {
                new SaleItem
                {
                    ProductId = command.Items[0].ProductId,
                    ProductName = command.Items[0].ProductName,
                    Quantity = command.Items[0].Quantity,
                    UnitPrice = command.Items[0].UnitPrice
                }
            }
        };

        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult { Id = sale.Id });

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        var item = sale.Items.ToList()[0];
        item.Discount.Should().Be(25m);  // 50 * 5 * 0.10
        item.Total.Should().Be(225m);    // (50 * 5) - 25
    }


    [Fact(DisplayName = "Given item with quantity < 4 When creating sale Then applies no discount")]
    public async Task Handle_QuantityLessThan4_AppliesNoDiscount()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = new List<SaleItemDto>()
        {
            new SaleItemDto
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Product A",
                Quantity = 2,
                UnitPrice = 30
            }
        };

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            Items = new List<SaleItem>
            {
                new SaleItem
                {
                    ProductId = command.Items[0].ProductId,
                    ProductName = command.Items[0].ProductName,
                    Quantity = command.Items[0].Quantity,
                    UnitPrice = command.Items[0].UnitPrice
                }
            }
        };

        _mapper.Map<Sale>(command).Returns(sale);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult { Id = sale.Id });

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        var item = sale.Items.ToList()[0];
        item.Discount.Should().Be(0m);
        item.Total.Should().Be(60m);     // 30 * 2
    }

}
