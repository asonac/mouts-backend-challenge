using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing CreateSaleCommand requests
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of CreateSaleHandler
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the CreateSaleCommand request
    /// </summary>
    /// <param name="command">The CreateSale command</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // Map command to domain entity
        var sale = _mapper.Map<Sale>(command);

        foreach (var item in sale.Items)
        {
            if (item.Quantity > 20)
                throw new InvalidOperationException($"Cannot sell more than 20 units of product {item.ProductName}");

            if (item.Quantity >= 10)
                item.Discount = item.UnitPrice * item.Quantity * 0.20m;
            else if (item.Quantity >= 4)
                item.Discount = item.UnitPrice * item.Quantity * 0.10m;
            else
                item.Discount = 0;

            item.Total = (item.UnitPrice * item.Quantity) - item.Discount;
        }

        sale.TotalAmount = sale.Items.Sum(i => i.Total);

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCreatedEvent(createdSale), cancellationToken);

        var result = _mapper.Map<CreateSaleResult>(createdSale);
        return result;
    }
}
