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
    public class MainController : Controller
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
            return View();
        }

        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb()
        {
            var podDetails = await FetchPODetailsAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-pod", podDetails);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "POD saved successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to save POD.";
            }
            return View("Index");
        }

        [HttpPost("save-poh")]
        public async Task<IActionResult> SavePOHToDb()
        {
            var pohHeaders = await FetchPOHeadersAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-poh", pohHeaders);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "POH saved successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to save POH.";
            }
            return View("Index");
        }

        [HttpPost("generate-xml")]
        public async Task<IActionResult> GenerateXml()
        {
            var response = await _fileManagementServiceClient.PostAsync("api/file/generate", null);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "XML generated successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to generate XML.";
            }
            return View("Index");
        }

        [HttpPost("send-xml")]
        public async Task<IActionResult> SendXml()
        {
            var response = await _sftpCommunicationServiceClient.PostAsync("api/sftp/upload", null);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "XML sent successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to send XML.";
            }
            return View("Index");
        }

        [HttpPost("fetch-xml")]
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
                    foreach (var summary in result.Details)
                    {
                        podDetails.Add(new PurchaseOrderDetailDto
                        {
                            PurchaseOrderDetailId = summary.PurchaseOrderDetailId,
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
