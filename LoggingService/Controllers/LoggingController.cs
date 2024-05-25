using LoggingService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LoggingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IHubContext<LogHub> _hubContext;

        public LogsController(IHubContext<LogHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LogMessage logMessage)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLogMessage", logMessage);
            return Ok();
        }
    }

    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
    }
}
