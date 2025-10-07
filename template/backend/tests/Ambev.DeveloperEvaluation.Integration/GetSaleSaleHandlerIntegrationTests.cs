using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
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

public class GetSaleHandlerIntegrationTests
{
    private readonly DefaultContext _context;
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _createHandler;
    private readonly GetSaleHandler _getHandler;

    public GetSaleHandlerIntegrationTests()
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
        _getHandler = new GetSaleHandler(_repository, _mapper);
    }

    [Fact(DisplayName = "Should retrieve sale with correct data")]
    public async Task Handle_ValidQuery_ReturnsSaleCorrectly()
    {
        var createCommand = CreateSaleHandlerTestData.GenerateValidCommand();
        createCommand.Items = new List<SaleItemDto>
        {
            new SaleItemDto
            {
                ProductId = Guid.NewGuid().ToString(),
                ProductName = "Test Product",
                Quantity = 5,
                UnitPrice = 100
            }
        };

        var createResult = await _createHandler.Handle(createCommand, CancellationToken.None);

        var query = new GetSaleCommand (createResult.Id);
        var getResult = await _getHandler.Handle(query, CancellationToken.None);

        getResult.Should().NotBeNull();
        getResult.Id.Should().Be(createResult.Id);
        getResult.CustomerName.Should().Be(createCommand.CustomerName);
        getResult.Items.Should().HaveCount(1);
        getResult.Items[0].ProductName.Should().Be("Test Product");
        getResult.Items[0].Discount.Should().Be(50); // 100 * 5 * 0.10
        getResult.TotalAmount.Should().Be(450);      // 500 - 50
    }

    [Fact(DisplayName = "Should throw exception when sale not found")]
    public async Task Handle_InvalidId_ThrowsException()
    {
        var query = new GetSaleCommand(Guid.NewGuid());

        var act = () => _getHandler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
