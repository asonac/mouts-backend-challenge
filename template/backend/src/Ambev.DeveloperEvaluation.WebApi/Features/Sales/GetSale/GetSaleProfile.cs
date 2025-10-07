using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>
/// AutoMapper profile for configuring mappings related to the GetSale feature.
/// </summary>
public class GetSaleProfile : Profile
{
    /// <summary>
    /// Configures mappings between API layer and application layer types for retrieving a sale.
    /// </summary>
    public GetSaleProfile()
    {
        CreateMap<Guid, GetSaleCommand>()
            .ConstructUsing(id => new GetSaleCommand(id));

        CreateMap<GetSaleResult, GetSaleResponse>();
        CreateMap<GetSaleItemDto, GetSaleItemResponse>();
    }
}
