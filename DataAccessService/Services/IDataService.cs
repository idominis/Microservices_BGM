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
    }
}
