using FrontendService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Threading.Tasks;

namespace FrontendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrontendController : Controller
    {
        private readonly HttpClient _orderManagementServiceClient;
        private readonly IHubContext<UpdateHub> _hubContext;

        public FrontendController(IHttpClientFactory httpClientFactory, IHubContext<UpdateHub> hubContext)
        {
            _orderManagementServiceClient = httpClientFactory.CreateClient("OrderManagementService");
            _hubContext = hubContext;
        }

        [HttpGet]
        [Route("/")] // Handle the root URL
        [Route("Frontend/Index")] // Also handle /Frontend/Index
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("notify-latest-date")]
        public async Task<IActionResult> NotifyLatestDate([FromBody] DateTime latestDate)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLatestDateUpdate", latestDate);
            return Ok();
        }

        [HttpPost("notify-latest-date-generated")]
        public async Task<IActionResult> NotifyLatestDateGenerated([FromBody] DateTime latestDate)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLatestDateGeneratedUpdate", latestDate);
            return Ok();
        }


        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb()
        {
            var response = await _orderManagementServiceClient.PostAsync("api/orders/save-pod", null);
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
            var response = await _orderManagementServiceClient.PostAsync("api/orders/save-poh", null);
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
            var response = await _orderManagementServiceClient.PostAsync("api/orders/generate-xml", null);
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
            var response = await _orderManagementServiceClient.PostAsync("api/orders/send-xml", null);
            if (response.IsSuccessStatusCode)
            {
                return Ok("XML sent successfully!");
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Failed to send XML.");
            }
        }
    }
}
