using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrderManagementService.DTO;

namespace OrderManagementService.Services
{
    public interface IOrderService
    {
        Task<string> GetPathAsync(string pathName);
        Task<bool> DownloadFilesPODAsync();
        Task<bool> DownloadFilesPOHAsync();
        Task<bool> SavePODetailsAsync(List<PurchaseOrderDetailDto> purchaseOrderDetailsDto);
        Task<bool> SavePOHeadersAsync(List<PurchaseOrderHeaderDto> purchaseOrderHeadersDto);
        //Task<bool> GenerateXmlAsync();
        Task<bool> GenerateXmlAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> SendXmlAsync();
        Task<bool> SendDateGeneratedXmlAsync(DateTime startDate, DateTime endDate);
        Task<List<PurchaseOrderDetailDto>> FetchPODetailsAsync();
        Task<List<PurchaseOrderHeaderDto>> FetchPOHeadersAsync();
        Task<(DateTime? earliestDate, DateTime? latestDate)> GetOrderDateRangeAsync();

    }
}
