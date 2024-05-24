using AutoMapper;
using DataAccessService.Data;
using DataAccessService.DTO;
using DataAccessService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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

        public async Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummariesByDateAsync(DateTime startDate, DateTime endDate)
        {
            var viewData = await _context.VPurchaseOrderSummaries
                .Where(p => p.OrderDate >= startDate && p.OrderDate <= endDate)
                .ToListAsync();

            return _mapper.Map<List<PurchaseOrderSummary>>(viewData);
        }

        public async Task<List<VPurchaseOrderSummary>> FetchSummariesToGenerateAsync(HashSet<int> alreadyGeneratedIds, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.VPurchaseOrderSummaries.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(s => s.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(s => s.OrderDate <= endDate.Value);
                }

                if (alreadyGeneratedIds != null && alreadyGeneratedIds.Count > 0)
                {
                    query = query.Where(s => !alreadyGeneratedIds.Contains(s.PurchaseOrderId));
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                Log.Error(ex, "An error occurred while fetching purchase order summaries to generate.");
                return new List<VPurchaseOrderSummary>();
            }
        }





        public async Task<HashSet<int>> FetchAlreadyGeneratedPurchaseOrderIdsAsync()
        {
            return new HashSet<int>(await _context.PurchaseOrdersProcessedSents
                .Where(x => x.OrderProcessed) // Filter for OrderProcessed = true
                .Select(x => x.PurchaseOrderId)
                .ToListAsync());
        }

        public async Task<HashSet<int>> FetchAlreadySentPurchaseOrderIdsAsync()
        {
            return new HashSet<int>(await _context.PurchaseOrdersProcessedSents
                .Where(x => x.OrderSent) // Filter for OrderSent = true
                .Select(x => x.PurchaseOrderId)
                .ToListAsync());
        }

        public async Task<bool> UpdatePurchaseOrderStatusAsync(int purchaseOrderId, int purchaseOrderDetailId, bool processed, bool sent, int channel)
        {
            try
            {
                var existingEntity = await _context.PurchaseOrdersProcessedSents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.PurchaseOrderDetailId == purchaseOrderDetailId);

                if (existingEntity != null)
                {
                    _context.PurchaseOrdersProcessedSents.Attach(existingEntity);
                    existingEntity.OrderProcessed = processed;
                    existingEntity.OrderSent = sent;
                    existingEntity.Channel = channel;
                    existingEntity.ModifiedDate = DateTime.Now;
                }
                else
                {
                    var newEntity = new PurchaseOrdersProcessedSent
                    {
                        PurchaseOrderId = purchaseOrderId,
                        PurchaseOrderDetailId = purchaseOrderDetailId,
                        OrderProcessed = processed,
                        OrderSent = sent,
                        Channel = channel,
                        ModifiedDate = DateTime.Now
                    };
                    _context.PurchaseOrdersProcessedSents.Add(newEntity);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions, such as constraint violations
                Log.Error(ex, "An error occurred while updating the purchase order status in the database.");
                return false;
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Log.Error(ex, "An unexpected error occurred while updating the purchase order status.");
                return false;
            }
        }

        public async Task<DateTime?> GetLatestDateForPurchaseOrderAsync(int purchaseOrderId)
        {
            var latestDate = await _context.VPurchaseOrderSummaries
                                              .Where(x => x.PurchaseOrderId == purchaseOrderId)
                                              .Select(x => x.OrderDate)
                                              .FirstOrDefaultAsync();
            return latestDate;
        }



    }
}
