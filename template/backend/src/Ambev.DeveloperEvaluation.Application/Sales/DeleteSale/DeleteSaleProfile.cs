using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Profile for mapping between DeleteSaleCommand, Sale entity, and DeleteSaleResult.
/// </summary>
public class DeleteSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for DeleteSale operation.
    /// </summary>
    public DeleteSaleProfile()
    {
        CreateMap<bool, DeleteSaleResult>().ConstructUsing(id => new DeleteSaleResult(id)); ;
    }
}
