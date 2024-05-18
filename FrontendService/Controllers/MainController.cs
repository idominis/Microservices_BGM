using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FrontendService.DTO;
using System.Collections.Generic;

namespace FrontendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly HttpClient _fileManagementServiceClient;
        private readonly HttpClient _sftpCommunicationServiceClient;
        private readonly HttpClient _dataAccessServiceClient;

        public MainController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _fileManagementServiceClient = httpClientFactory.CreateClient("FileManagementServiceClient");
            _sftpCommunicationServiceClient = httpClientFactory.CreateClient("SFTPCommunicationServiceClient");
            _dataAccessServiceClient = httpClientFactory.CreateClient("DataAccessServiceClient");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Index action is temporarily disabled for view rendering.");
        }

        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb()
        {
            var podDetails = await FetchPODetailsAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-pod", podDetails);
            if (response.IsSuccessStatusCode)
            {
                return Ok("POD saved successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to save POD.");
            }
        }

        [HttpPost("save-poh")]
        public async Task<IActionResult> SavePOHToDb()
        {
            var pohHeaders = await FetchPOHeadersAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-poh", pohHeaders);
            if (response.IsSuccessStatusCode)
            {
                return Ok("POH saved successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to save POH.");
            }
        }

        [HttpPost("generate-xml")]
        public async Task<IActionResult> GenerateXml()
        {
            var response = await _fileManagementServiceClient.PostAsync("api/file/generate", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok("XML generated successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to generate XML.");
            }
        }

        [HttpPost("send-xml")]
        public async Task<IActionResult> SendXml()
        {
            var response = await _sftpCommunicationServiceClient.PostAsync("api/sftp/upload", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok("XML sent successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to send XML.");
            }
        }

        private async Task<List<PurchaseOrderDetailDto>> FetchPODetailsAsync()
        {
            var filePath = "C:\\Users\\ido\\Documents\\BGM_project\\local\\data_received\\2011-04-23\\PurchaseOrderDetail1.xml";
            var typeName = "FileManagementService.Models.PurchaseOrderDetails"; // Adjust the type name accordingly

            var response = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/load-xml?filePath={Uri.EscapeDataString(filePath)}&typeName={Uri.EscapeDataString(typeName)}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PurchaseOrderDetails>();
                if (result != null)
                {
                    var podDetails = new List<PurchaseOrderDetailDto>();
                    foreach (var detail in result.Details)
                    {
                        podDetails.Add(new PurchaseOrderDetailDto
                        {
                            PurchaseOrderDetailId = detail.PurchaseOrderDetailId,
                            PurchaseOrderId = detail.PurchaseOrderId,
                            DueDate = detail.DueDate,
                            OrderQty = detail.OrderQty,
                            ProductId = detail.ProductId,   
                            UnitPrice = detail.UnitPrice,
                            LineTotal = detail.LineTotal,
                            ReceivedQty = detail.ReceivedQty,
                            RejectedQty = detail.RejectedQty,
                            StockedQty = detail.StockedQty,
                            ModifiedDate = detail.ModifiedDate
                        });
                    }
                    return podDetails;
                }
            }

            return new List<PurchaseOrderDetailDto>();
        }

        private async Task<List<PurchaseOrderHeaderDto>> FetchPOHeadersAsync()
        {
            return new List<PurchaseOrderHeaderDto>();
        }
    }
}
