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


    }
}
