namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>
/// Request object for retrieving a sale by its unique identifier.
/// </summary>
public class GetSaleRequest
{
    /// <summary>
    /// The unique identifier of the sale to retrieve.
    /// </summary>
    public Guid Id { get; set; }
}
