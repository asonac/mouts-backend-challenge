using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHandler"/> class.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid sale ID When querying Then returns sale result")]
    public async Task Handle_ValidId_ReturnsSaleResult()
    {
        // Given
        var sale = GetSaleHandlerTestData.GenerateSaleEntity();
        var query = new GetSaleCommand(sale.Id);
        var expectedResult = new GetSaleResult { Id = sale.Id };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given sale entity When querying Then maps to GetSaleResult")]
    public async Task Handle_MapsSaleToResult()
    {
        // Given
        var sale = GetSaleHandlerTestData.GenerateSaleEntity();
        var query = new GetSaleCommand(sale.Id);
        var expectedResult = new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            CustomerName = sale.CustomerName,
            BranchName = sale.BranchName,
            TotalAmount = sale.TotalAmount
        };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(query, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<GetSaleResult>(sale);
        result.Should().BeEquivalentTo(expectedResult);
    }
}
