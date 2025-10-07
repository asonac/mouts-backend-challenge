using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Integration.Config
{

    /// <summary>
    /// AutoMapper profile for mapping between domain entities and application models.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateSale
            CreateMap<CreateSaleCommand, Sale>()
    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<Sale, CreateSaleResult>();
            CreateMap<SaleItemDto, SaleItem>();

            // UpdateSale
            CreateMap<UpdateSaleCommand, Sale>()
           .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            CreateMap<UpdateSaleItemDto, SaleItem>();
            CreateMap<Sale, UpdateSaleResult>();

            // GetSale
            CreateMap<Sale, GetSaleResult>();
            CreateMap<SaleItem, GetSaleItemDto>();

            // DeleteSale
            CreateMap<Sale, DeleteSaleResult>();
        }
    }
}
