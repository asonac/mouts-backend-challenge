namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Represents the response returned after successfully creating a new user.
/// </summary>
/// <remarks>
/// This response contains the unique identifier of the newly Updated user,
/// which can be used for subsequent operations or reference.
/// </remarks>
public class UpdateSaleResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the newly Updated user.
    /// </summary>
    /// <value>A GUID that uniquely identifies the Updated user in the system.</value>
    public Guid Id { get; set; }
}
