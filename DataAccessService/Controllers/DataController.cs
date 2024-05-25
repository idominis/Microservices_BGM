using AutoMapper;
using DataAccessService.Data;
using DataAccessService.DTO;
using DataAccessService.Models;
using DataAccessService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IDataService _dataService;

        public DataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb([FromBody] List<PurchaseOrderDetailDto> podDetails)
        {
            var result = await _dataService.SavePODToDbAsync(podDetails);
            if (result == "Purchase order details saved successfully!")
            {
                return Ok(result);
            }
            else if (result == "No purchase order details provided." || result == "No new purchase orders details to save.")
            {
                return BadRequest(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpPost("save-poh")]
        public async Task<IActionResult> SavePOHToDb([FromBody] List<PurchaseOrderHeaderDto> podHeaders)
        {
            var result = await _dataService.SavePOHToDbAsync(podHeaders);
            if (result == "Purchase order headers saved successfully!")
            {
                return Ok(result);
            }
            else if (result == "No purchase order headers provided." || result == "No new purchase orders headers to save.")
            {
                return BadRequest(result);
            }
            else
            {
                return StatusCode(500, result);
            }
        }

        [HttpGet("fetch-summaries")]
        public async Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummaries()
        {
            return await _dataService.FetchPurchaseOrderSummariesAsync();
        }

        [HttpPost("fetch-summaries-date")]
        public async Task<List<PurchaseOrderSummary>> FetchPurchaseOrderSummariesByDate([FromBody] DateRangeDto dateRange)
        {
            return await _dataService.FetchPurchaseOrderSummariesByDateAsync(dateRange.StartDate, dateRange.EndDate);
        }

        [HttpPost("fetch-summaries-to-generate")]
        public async Task<ActionResult<List<PurchaseOrderSummary>>> FetchSummariesToGenerate([FromBody] FetchSummariesRequestDto request)
        {
            var summaries = await _dataService.FetchSummariesToGenerateAsync(request.AlreadyGeneratedIds, request.StartDate, request.EndDate);
            return Ok(summaries);
        }


        [HttpGet("fetch-generated")]
        public async Task<HashSet<int>> FetchAlreadyGeneratedPurchaseOrderIds()
        {
            return await _dataService.FetchAlreadyGeneratedPurchaseOrderIdsAsync();
        }

        [HttpGet("fetch-sent")]
        public async Task<HashSet<int>> FetchAlreadySentPurchaseOrderIds()
        {
            return await _dataService.FetchAlreadySentPurchaseOrderIdsAsync();
        }

        [HttpPut("update-po-status/{purchaseOrderId}/{purchaseOrderDetailId}/{processed}/{sent}/{channel}")] //TODO
        public async Task<bool> UpdatePurchaseOrderStatus(int purchaseOrderId, int purchaseOrderDetailId, bool processed, bool sent, int channel)
        {
            return await _dataService.UpdatePurchaseOrderStatusAsync(purchaseOrderId, purchaseOrderDetailId, processed, sent, channel);
        }

        [HttpGet("get-po-latest-date/{purchaseOrderId}")]
        public async Task<DateTime?> GetLatestDateForPurchaseOrder(int purchaseOrderId)
        {
            return await _dataService.GetLatestDateForPurchaseOrderAsync(purchaseOrderId);
        }

        [HttpGet("get-po-latest-sent-date/{purchaseOrderId}")]
        public async Task<DateTime?> GetLatestDateSentForPurchaseOrder(int purchaseOrderId)
        {
            return await _dataService.GetLatestDateSentForPurchaseOrderAsync(purchaseOrderId);
        }

        [HttpGet("get-po-latest-generated-date/{purchaseOrderId}")]
        public async Task<DateTime?> GetLatestDateGeneratedForPurchaseOrder(int purchaseOrderId)
        {
            return await _dataService.GetLatestDateGeneratedForPurchaseOrderAsync(purchaseOrderId);
        }

        [HttpGet("effective-date-range")]
        public async Task<IActionResult> GetEffectiveDateRange()
        {
            var (earliestDate, latestDate) = await _dataService.GetEffectiveDateRangeAsync();
            return Ok(new { earliestDate, latestDate });
        }


    }
}
