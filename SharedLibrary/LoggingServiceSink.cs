using Serilog.Core;
using Serilog.Events;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class LoggingServiceSink : ILogEventSink
    {
        private readonly HttpClient _httpClient;
        private readonly string _loggingServiceUrl;

        public LoggingServiceSink(HttpClient httpClient, string loggingServiceUrl)
        {
            _httpClient = httpClient;
            _loggingServiceUrl = loggingServiceUrl;
        }

        public void Emit(LogEvent logEvent)
        {
            var logMessage = new
            {
                Timestamp = logEvent.Timestamp,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString()
            };

            var content = new StringContent(JsonSerializer.Serialize(logMessage), Encoding.UTF8, "application/json");
            _httpClient.PostAsync(_loggingServiceUrl, content).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
