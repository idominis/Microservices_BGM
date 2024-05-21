using AutoMapper;
using DataAccessService.Data;
using DataAccessService.DTO;
using DataAccessService.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessService.Services
{
    public class DataService : IDataService
    {
        private readonly BgmDbContext _context;
        private readonly IMapper _mapper;

        public DataService(BgmDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<string> SavePODToDbAsync(List<PurchaseOrderDetailDto> podDetails)
        {
            if (podDetails == null || !podDetails.Any())
            {
                return "No purchase order details provided.";
            }

            var existingIds = new HashSet<int>(_context.PurchaseOrderDetails.Select(p => p.PurchaseOrderDetailId));
            var newDetails = podDetails
                .Where(dto => !existingIds.Contains(dto.PurchaseOrderDetailId))
                .Select(dto => _mapper.Map<PurchaseOrderDetail>(dto))
                .ToList();

            if (newDetails.Any())
            {
                await _context.PurchaseOrderDetails.AddRangeAsync(newDetails);
                try
                {
                    await _context.SaveChangesAsync();
                    string detailIds = string.Join(", ", newDetails.Select(h => h.PurchaseOrderDetailId));
                    Log.Information($"Purchase order details: {detailIds} loaded and saved successfully!");
                    return "Purchase order details saved successfully!";
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, "Error updating the database with new purchase order details.");
                    return "Internal server error";
                }
            }
            else
            {
                return "No new purchase orders details to save.";
            }
        }

        public async Task<string> SavePOHToDbAsync(List<PurchaseOrderHeaderDto> podHeaders)
        {
            if (podHeaders == null || !podHeaders.Any())
            {
                return "No purchase order headers provided.";
            }

            var existingIds = new HashSet<int>(_context.PurchaseOrderHeaders.Select(p => p.PurchaseOrderId));
            var newHeaders = podHeaders
                .Where(dto => !existingIds.Contains(dto.PurchaseOrderId))
                .Select(dto => _mapper.Map<PurchaseOrderHeader>(dto))
                .ToList();

            if (newHeaders.Any())
            {
                await _context.PurchaseOrderHeaders.AddRangeAsync(newHeaders);
                try
                {
                    await _context.SaveChangesAsync();
                    string headerIds = string.Join(", ", newHeaders.Select(h => h.PurchaseOrderId));
                    Log.Information($"Purchase order headers: {headerIds} loaded and saved successfully!");
                    return "Purchase order headers saved successfully!";
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, "Error updating the database with new purchase order headers.");
                    return "Internal server error";
                }
            }
            else
            {
                return "No new purchase orders headers to save.";
            }
        }

        public async Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummariesAsync()
        {
            var viewData = await _context.VPurchaseOrderSummaries.ToListAsync();
            return viewData.Select(v => new PurchaseOrderSummary
            {
                PurchaseOrderID = v.PurchaseOrderId,
                PurchaseOrderDetailID = v.PurchaseOrderDetailId,
                OrderDate = v.OrderDate,
                VendorID = v.VendorId,
                VendorName = v.VendorName,
                ProductID = v.ProductId,
                ProductNumber = v.ProductNumber,
                ProductName = v.ProductName,
                OrderQty = v.OrderQty,
                UnitPrice = v.UnitPrice,
                LineTotal = v.LineTotal,
                SubTotal = v.SubTotal,
                TaxAmt = v.TaxAmt,
                Freight = v.Freight,
                TotalDue = v.TotalDue
            }).ToList();
        }
    }
}
