using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
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

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly UpdateSaleHandler _handler;
    private readonly UpdateSaleCommandValidator _validator = new();

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _eventPublisher);
    }

    [Fact(DisplayName = "Given valid sale update When handling Then returns success response")]
    public async Task Handle_ValidUpdate_ReturnsSuccessResponse()
    {
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        var existingSale = new Sale { Id = command.Id, Items = new List<SaleItem>() };
        var result = new UpdateSaleResult { Id = existingSale.Id };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map(command, existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(existingSale.Id);
    }

    [Fact(DisplayName = "Given invalid update data When handling Then throws validation exception")]
    public async Task Handle_InvalidUpdate_ThrowsValidationException()
    {
        var command = new UpdateSaleCommand(); // Empty

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given nonexistent sale ID When updating Then throws not found exception")]
    public async Task Handle_NonexistentSale_ThrowsInvalidOperationException()
    {
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();

         _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Sale with ID {command.Id} not found.");
    }

    [Fact(DisplayName = "Given item with quantity > 20 When updating sale Then throws validation exception")]
    public async Task Handle_ExcessiveQuantity_ThrowsValidationException()
    {
        var command = UpdateSaleHandlerTestData.GenerateCommandWithExcessiveQuantity();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cannot sell more than 20 identical items*");
    }


    [Fact(DisplayName = "Given cancelled sale When updating Then publishes SaleCancelledEvent")]
    public async Task Handle_CancelledSale_PublishesSaleCancelledEvent()
    {
        var command = UpdateSaleHandlerTestData.GenerateCancelledCommand();
        var existingSale = new Sale { Id = command.Id, Items = new List<SaleItem>(), IsCancelled = false };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map(command, existingSale);
        existingSale.IsCancelled = true;
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult { Id = existingSale.Id });

        await _handler.Handle(command, CancellationToken.None);

        await _eventPublisher.Received(1).PublishAsync(Arg.Is<SaleCancelledEvent>(e => e.Sale.Id == existingSale.Id), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given modified sale When updating Then publishes SaleModifiedEvent")]
    public async Task Handle_ModifiedSale_PublishesSaleModifiedEvent()
    {
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        var existingSale = new Sale { Id = command.Id, Items = new List<SaleItem>(), IsCancelled = false };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map(command, existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult { Id = existingSale.Id });

        await _handler.Handle(command, CancellationToken.None);

        await _eventPublisher.Received(1).PublishAsync(Arg.Is<SaleModifiedEvent>(e => e.Sale.Id == existingSale.Id), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given item marked as cancelled When updating Then publishes ItemCancelledEvent")]
    public async Task Handle_CancelledItem_PublishesItemCancelledEvent()
    {
        var command = UpdateSaleHandlerTestData.GenerateCommandWithCancelledItem();
        var existingSale = new Sale
        {
            Id = command.Id,
            Items = command.Items.Select(i => new SaleItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                IsCancelled = i.IsCancelled
            }).ToList()
        };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map(command, existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult { Id = existingSale.Id });

        await _handler.Handle(command, CancellationToken.None);

        await _eventPublisher.Received(1).PublishAsync(Arg.Is<ItemCancelledEvent>(e => e.Item.ProductId == command.Items[0].ProductId), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given item with quantity between 4 and 9 When updating sale Then applies 10% discount")]
    public async Task Handle_QuantityBetween4And9_Applies10PercentDiscount()
    {
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        command.Items = new List<UpdateSaleItemDto>
        {
            new UpdateSaleItemDto
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Product B",
                Quantity = 5,
                UnitPrice = 50
            }
        };

        var existingSale = new Sale
        {
            Id = command.Id,
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

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map(command, existingSale);
        _saleRepository.UpdateAsync(existingSale, Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(new UpdateSaleResult { Id = existingSale.Id });

        var result = await _handler.Handle(command, CancellationToken.None);

        var item = existingSale.Items.ToList()[0];
        item.Discount.Should().Be(25m);  // 50 * 5 * 0.10
        item.Total.Should().Be(225m);    // (50 * 5) - 25
    }
}
