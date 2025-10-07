using Ambev.DeveloperEvaluation.Domain.Common.Interfaces;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing UpdateSaleCommand requests
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingSale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingSale == null)
            throw new InvalidOperationException($"Sale with ID {command.Id} not found.");

        _mapper.Map(command, existingSale);

        foreach (var item in existingSale.Items)
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

        existingSale.TotalAmount = existingSale.Items.Sum(i => i.Total);

        existingSale.UpdatedAt = DateTime.UtcNow;

        var updatedSale = await _saleRepository.UpdateAsync(existingSale, cancellationToken);
        var result = _mapper.Map<UpdateSaleResult>(updatedSale);

        if (updatedSale.IsCancelled)
        {
            await _eventPublisher.PublishAsync(new SaleCancelledEvent(updatedSale), cancellationToken);
        }
        else
        {
            await _eventPublisher.PublishAsync(new SaleModifiedEvent(updatedSale), cancellationToken);
        }

        foreach (var item in updatedSale.Items.Where(i => i.IsCancelled))
        {
            await _eventPublisher.PublishAsync(new ItemCancelledEvent(item), cancellationToken);
        }

        return result;
    }
}