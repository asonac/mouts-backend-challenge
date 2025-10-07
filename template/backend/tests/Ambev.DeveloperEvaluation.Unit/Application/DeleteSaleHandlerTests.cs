using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="DeleteSaleHandler"/> class.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly DeleteSaleHandler _handler;
    private readonly DeleteSaleCommandValidator _validator = new();

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new DeleteSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given valid sale ID When deleting Then returns success result")]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Given
        var command = DeleteSaleHandlerTestData.GenerateValidCommand();
        var sale = DeleteSaleHandlerTestData.GenerateSaleEntity();
        var expectedResult = new DeleteSaleResult(true);
        

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.DeleteAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(true);
        _mapper.Map<DeleteSaleResult>(true).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given nonexistent sale ID When deleting Then throws not found exception")]
    public async Task Handle_SaleNotFound_ThrowsInvalidOperationException()
    {
        // Given
        var command = DeleteSaleHandlerTestData.GenerateCommandWithInvalidId();

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Sale with ID {command.Id} not found.");
    }

    [Fact(DisplayName = "Should pass validation for valid delete command")]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = DeleteSaleHandlerTestData.GenerateValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Should fail validation when ID is empty")]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new DeleteSaleCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
