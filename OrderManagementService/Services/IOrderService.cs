using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrderManagementService.DTO;

namespace OrderManagementService.Services
{
    public interface IOrderService
    {
        Task<bool> SavePODetailsAsync(List<PurchaseOrderDetailDto> purchaseOrderDetailsDto);
        Task<bool> SavePOHeadersAsync(List<PurchaseOrderHeaderDto> purchaseOrderHeadersDto);
        Task<bool> GenerateXmlAsync();
        Task<bool> SendXmlAsync();
        Task<List<PurchaseOrderDetailDto>> FetchPODetailsAsync();
        Task<List<PurchaseOrderHeaderDto>> FetchPOHeadersAsync();
        Task<string> GetPathAsync(string pathName);
    }
}
