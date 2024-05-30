using Microsoft.AspNetCore.SignalR;

namespace FrontendService.Hubs
{
    public class UpdateHub : Hub
    {
        // Method to send the latest date update to clients
        public async Task SendLatestDateSentUpdate(DateTime date)
        {
            await Clients.All.SendAsync("ReceiveLatestDateSentUpdate", date);
        }

        // Method to send the latest generated date update to clients
        public async Task SendLatestDateGeneratedUpdate(DateTime date)
        {
            await Clients.All.SendAsync("ReceiveLatestDateGeneratedUpdate", date);
        }
    }
}
