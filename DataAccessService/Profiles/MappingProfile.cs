using AutoMapper;
using DataAccessService.DTO;
using DataAccessService.Models;

namespace DataAccessService.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PurchaseOrderDetailDto, PurchaseOrderDetail>().ReverseMap();
            CreateMap<PurchaseOrderHeaderDto, PurchaseOrderHeader>().ReverseMap();
            CreateMap<VPurchaseOrderSummary, PurchaseOrderSummary>().ReverseMap();
        }
    }
}
