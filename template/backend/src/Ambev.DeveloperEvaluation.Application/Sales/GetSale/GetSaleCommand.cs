using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Command for creating a new sale.
/// </summary>
/// <remarks>
/// This command captures all necessary data to register a sale, including sale number, date,
/// customer and branch info, and a list of items. It implements <see cref="IRequest{TResponse}"/>
/// to initiate the request that returns a <see cref="GetSaleResult"/>.
/// 
/// The data is validated using <see cref="GetSaleCommandValidator"/> which extends
/// <see cref="AbstractValidator{T}"/> to enforce business rules and field integrity.
/// </remarks>
public class GetSaleCommand : IRequest<GetSaleResult>
{
    /// <summary>
    /// Gets or sets the unique sale id.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Initializes a new instance of GetUserCommand
    /// </summary>
    /// <param name="id">The ID of the user to retrieve</param>
    public GetSaleCommand(Guid id)
    {
        Id = id;
    }
    /// <summary>
    /// Validates the command using FluentValidation.
    /// </summary>
    /// <returns>Validation result details</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new GetSaleCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
