using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;

/// <summary>
/// AutoMapper profile for configuring mappings related to the DeleteSale feature.
/// </summary>
public class DeleteSaleProfile : Profile
{
    /// <summary>
    /// Configures mappings between API layer and application layer types for retrieving a sale.
    /// </summary>
    public DeleteSaleProfile()
    {
        CreateMap<Guid, DeleteSaleCommand>()
            .ConstructUsing(id => new DeleteSaleCommand(id));
    }
}
