using Microsoft.AspNetCore.Mvc;
using OrderManagementService.Services;
using Serilog;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OrderManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("get-path")]
        public async Task<IActionResult> GetPath([FromQuery] string pathName)
        {
            try
            {
                var path = await _orderService.GetPathAsync(pathName);
                return Ok(path);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("download-file-pod")]
        public async Task DownloadFilesPODAsync()
        {
            var result = await _orderService.DownloadFilesPODAsync();

            if (result)
            {
                Log.Information("POD files downloaded successfully.");
            }
            else
            {
                Log.Error("Failed to download POD files.");
            }
        }

        [HttpPost("download-file-poh")]
        public async Task DownloadFilesPOHAsync()
        {
            var result = await _orderService.DownloadFilesPOHAsync();

            if (result)
            {
                Log.Information("POH files downloaded successfully.");
            }
            else
            {
                Log.Error("Failed to download POH files.");
            }
        }

        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb()
        {
            // check if there are files in local folder
            var ordersDetails = await _orderService.FetchPODetailsAsync();

            if (ordersDetails == null || ordersDetails.Count == 0)
            {
                return BadRequest("No purchase order details found in local folder.");
            }
            else
            { 
                var result = await _orderService.SavePODetailsAsync(ordersDetails);
                if (result)
                {
                    return Ok("POD saved successfully!");
                }
                else
                {
                    return StatusCode(500, "Failed to save POD.");
                }
            }
        }

        [HttpPost("save-poh")]
        public async Task<IActionResult> SavePOHToDb()
        {
            var ordersHeaders = await _orderService.FetchPOHeadersAsync();

            if (ordersHeaders == null || ordersHeaders.Count == 0)
            {
                return BadRequest("No purchase order headers found in local folder.");
            }
            else
            {
                var result = await _orderService.SavePOHeadersAsync(ordersHeaders);
                if (result)
                {
                    return Ok("POH saved successfully!");
                }
                else
                {
                    return StatusCode(500, "Failed to save POH.");
                }
            }
        }

        [HttpPost("generate-xml")]
        public async Task<IActionResult> GenerateXml()
        {
            var result = await _orderService.GenerateXmlAsync();
            if (result)
            {
                return Ok("XML generated successfully!");
            }
            else
            {
                return StatusCode(500, "Failed to generate XML.");
            }
        }

        [HttpPost("send-xml")]
        public async Task<IActionResult> SendXml()
        {
            var result = await _orderService.SendXmlAsync();
            if (result)
            {
                return Ok("XML sent successfully!");
            }
            else
            {
                return StatusCode(500, "Failed to send XML.");
            }
        }

        
    }
}
