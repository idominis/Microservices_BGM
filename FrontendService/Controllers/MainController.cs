using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FrontendService.DTO;

namespace FrontendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _fileManagementServiceUrl;
        private readonly string _sftpCommunicationServiceUrl;
        private readonly string _dataAccessServiceUrl;

        public MainController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _fileManagementServiceUrl = configuration["BaseAddresses:FileManagementService"];
            _sftpCommunicationServiceUrl = configuration["BaseAddresses:SFTPCommunicationService"];
            _dataAccessServiceUrl = configuration["BaseAddresses:DataAccessService"];
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
            var response = await _httpClient.PostAsJsonAsync($"{_dataAccessServiceUrl}/api/data/save-pod", podDetails);
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
            var response = await _httpClient.PostAsJsonAsync($"{_dataAccessServiceUrl}/api/data/save-poh", pohHeaders);
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
            var response = await _httpClient.PostAsync($"{_fileManagementServiceUrl}/api/file/generate", null);
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
            var response = await _httpClient.PostAsync($"{_sftpCommunicationServiceUrl}/api/sftp/upload", null);
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

        private async Task<List<PurchaseOrderDetailDto>> FetchPODetailsAsync()
        {
            // Fetch POD details from somewhere
            return new List<PurchaseOrderDetailDto>();
        }

        private async Task<List<PurchaseOrderHeaderDto>> FetchPOHeadersAsync()
        {
            // Fetch POH headers from somewhere
            return new List<PurchaseOrderHeaderDto>();
        }
    }
}
