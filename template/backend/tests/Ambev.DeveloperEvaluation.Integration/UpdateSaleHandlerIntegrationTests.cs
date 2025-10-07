using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
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

public class UpdateSaleHandlerIntegrationTests
{
    private readonly DefaultContext _context;
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _createHandler;
    private readonly UpdateSaleHandler _updateHandler;

    public UpdateSaleHandlerIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DefaultContext(options);
        _repository = new SaleRepository(_context);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = config.CreateMapper();
        _eventPublisher = Substitute.For<IEventPublisher>();

        _createHandler = new CreateSaleHandler(_repository, _mapper, _eventPublisher);
        _updateHandler = new UpdateSaleHandler(_repository, _mapper, _eventPublisher);
    }

    [Fact(DisplayName = "Should update sale and recalculate total correctly")]
    public async Task Handle_ValidCommand_UpdatesSaleCorrectly()
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

        var result = await _createHandler.Handle(command, CancellationToken.None);

        var commandUpdate = new UpdateSaleCommand
        {
            Id = result.Id,
            SaleNumber = "2134",
            SaleDate = DateTime.UtcNow,
            CustomerId = "5623424",
            CustomerName = "Updated Customer",
            BranchId = "1231234",
            BranchName = "Updated Branch",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = "Updated Product",
                    Quantity = 6,
                    UnitPrice = 100
                }
            }
        };

        var updateResult = await _updateHandler.Handle(commandUpdate, CancellationToken.None);

        var updatedSale = await _context.Sales.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == result.Id);
        updatedSale.Should().NotBeNull();
        updatedSale?.CustomerName.Should().Be("Updated Customer");
        updatedSale?.Items.ToList()[0].Discount.Should().Be(60); // 100 * 6 * 0.10
        updatedSale?.TotalAmount.Should().Be(540);      // 600 - 60
    }

    [Fact(DisplayName = "Should throw validation exception for empty command")]
    public async Task Handle_EmptyCommand_ThrowsValidationException()
    {
        var command = new UpdateSaleCommand(); // sem dados

        var act = () => _updateHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Should publish SaleUpdatedEvent after successful update")]
    public async Task Handle_ValidCommand_PublishesSaleUpdatedEvent()
    {
        var createCommand = CreateSaleHandlerTestData.GenerateValidCommand();
        var createResult = await _createHandler.Handle(createCommand, CancellationToken.None);

        var updateCommand = new UpdateSaleCommand
        {
            Id = createResult.Id,
            SaleNumber = createCommand.SaleNumber,
            SaleDate = createCommand.SaleDate,
            CustomerId = createCommand.CustomerId,
            CustomerName = "Updated Name",
            BranchId = createCommand.BranchId,
            BranchName = "Updated Branch",
            Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = "Updated Product",
                    Quantity = 4,
                    UnitPrice = 100
                }
            }
        };

        var updateResult = await _updateHandler.Handle(updateCommand, CancellationToken.None);

        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<SaleModifiedEvent>(e => e.Sale.Id == updateResult.Id),
            Arg.Any<CancellationToken>()
        );
    }
}
