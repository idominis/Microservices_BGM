using Microsoft.AspNetCore.SignalR.Client;
using Serilog.Core;
using Serilog.Events;
using System;

namespace OrderManagementService.Logging
{
    public class SignalRSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly HubConnection _hubConnection;

        public SignalRSink(IFormatProvider formatProvider, string hubUrl)
        {
            _formatProvider = formatProvider;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

            _hubConnection.StartAsync().Wait();
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            _hubConnection.InvokeAsync("SendLogMessage", message);
        }
    }
}
