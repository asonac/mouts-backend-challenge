using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Command for creating a new sale.
/// </summary>
/// <remarks>
/// This command captures all necessary data to register a sale, including sale number, date,
/// customer and branch info, and a list of items. It implements <see cref="IRequest{TResponse}"/>
/// to initiate the request that returns a <see cref="DeleteSaleResult"/>.
/// 
/// The data is validated using <see cref="DeleteSaleCommandValidator"/> which extends
/// <see cref="AbstractValidator{T}"/> to enforce business rules and field integrity.
/// </remarks>
public class DeleteSaleCommand : IRequest<DeleteSaleResult>
{
    /// <summary>
    /// Gets or sets the unique sale id.
    /// </summary>
    public Guid Id { get; set; }

    public DeleteSaleCommand(Guid id)
    {
        Id = id;
    }

    public ValidationResultDetail Validate()
    {
        var validator = new DeleteSaleCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
