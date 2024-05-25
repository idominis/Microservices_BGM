using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LoggingService.Hubs
{
    public class LogHub : Hub
    {
        public async Task SendLogMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveLogMessage", message);
        }
    }
}
