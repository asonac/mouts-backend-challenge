using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
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

public class DeleteSaleHandlerIntegrationTests
{
    private readonly DefaultContext _context;
    private readonly ISaleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _createHandler;
    private readonly DeleteSaleHandler _deleteHandler;

    public DeleteSaleHandlerIntegrationTests()
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
        _deleteHandler = new DeleteSaleHandler(_repository, _mapper);
    }

    [Fact(DisplayName = "Should delete sale and its items")]
    public async Task Handle_ValidCommand_DeletesSaleAndItems()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var result = await _createHandler.Handle(command, CancellationToken.None);

        var deleteCommand = new DeleteSaleCommand(result.Id);
        var deleteResult = await _deleteHandler.Handle(deleteCommand, CancellationToken.None);

        var deletedSale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == result.Id);
        var deletedItems = await _context.SaleItems.Where(i => i.Sale.Id == result.Id).ToListAsync();

        deletedSale.Should().BeNull();
        deletedItems.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should throw exception when sale not found")]
    public async Task Handle_InvalidId_ThrowsException()
    {
        var command = new DeleteSaleCommand(Guid.NewGuid());

        var act = () => _deleteHandler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
