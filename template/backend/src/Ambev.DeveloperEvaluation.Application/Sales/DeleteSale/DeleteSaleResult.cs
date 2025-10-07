namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Represents the response returned after successfully creating a new user.
/// </summary>
/// <remarks>
/// This response contains the unique identifier of the newly Deleted user,
/// which can be used for subsequent operations or reference.
/// </remarks>
public class DeleteSaleResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the newly Deleted user.
    /// </summary>
    /// <value>A GUID that uniquely identifies the Deleted user in the system.</value>00
    public bool Result { get; set; }

    public DeleteSaleResult(bool result)
    {
        Result = result;
    }
}
