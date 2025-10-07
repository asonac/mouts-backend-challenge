using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Config;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Application;

public class CreateSaleHandlerIntegrationTests
{
    private readonly DefaultContext _context;
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Ensure the InMemory provider is referenced
            .Options;

        _context = new DefaultContext(options);
        _repository = new SaleRepository(_context);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>(); // seu perfil de mapeamento
        });

        _mapper = config.CreateMapper();
        _eventPublisher = Substitute.For<IEventPublisher>();

        _handler = new CreateSaleHandler(_repository, _mapper, _eventPublisher);
    }

    [Fact(DisplayName = "Should persist sale and calculate total correctly")]
    public async Task Handle_ValidCommand_PersistsSaleCorrectly()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        var savedSale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == result.Id);
        savedSale.Should().NotBeNull();
        savedSale.TotalAmount.Should().BeGreaterThan(0);
        savedSale.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Should throw validation exception for empty command")]
    public async Task Handle_EmptyCommand_ThrowsValidationException()
    {
        var command = new CreateSaleCommand(); // sem dados

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Should apply 10% discount when quantity is between 4 and 9")]
    public async Task Handle_QuantityBetween4And9_Applies10PercentDiscount()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items = new List<SaleItemDto>
        {
            new SaleItemDto
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Test Product",
                Quantity = 5,
                UnitPrice = 100
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        var sale = await _context.Sales.FirstOrDefaultAsync(i => i.Id == result.Id);
        sale?.Items.ToList().Should().NotBeNull();
        sale?.Items.ToList()[0].Discount.Should().Be(50); // 100 * 5 * 0.10
        sale?.TotalAmount.Should().Be(450);   // 500 - 50
    }


    [Fact(DisplayName = "Should publish SaleCreatedEvent after successful creation")]
    public async Task Handle_ValidCommand_PublishesSaleCreatedEvent()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<SaleCreatedEvent>(e => e.Sale.Id == result.Id),
            Arg.Any<CancellationToken>()
        );
    }
}
