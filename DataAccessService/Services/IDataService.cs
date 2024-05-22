using DataAccessService.DTO;
using DataAccessService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessService.Services
{
    public interface IDataService
    {
        Task<string> SavePODToDbAsync(List<PurchaseOrderDetailDto> podDetails);
        Task<string> SavePOHToDbAsync(List<PurchaseOrderHeaderDto> podHeaders);
        Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummariesAsync();
        Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummariesByDateAsync(DateTime startDate, DateTime endDate);
        Task<HashSet<int>> FetchAlreadyGeneratedPurchaseOrderIdsAsync();
        Task<HashSet<int>> FetchAlreadySentPurchaseOrderIdsAsync();
        Task<bool> UpdatePurchaseOrderStatusAsync(int purchaseOrderId, int purchaseOrderDetailId, bool processed, bool sent, int channel);
        Task<DateTime?> GetLatestDateForPurchaseOrderAsync(int purchaseOrderId);
    }
}
