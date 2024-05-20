using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace SFTPCommunicationService.Services
{
    public class SftpClientManager
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly SftpConfig _config;

        public SftpClientManager(SftpConfig config)
        {
            _config = config;
        }

        public SftpClient Connect()
        {
            var connectionInfo = new ConnectionInfo(_config.Host, _config.Username,
                new PasswordAuthenticationMethod(_config.Username, _config.Password));

            var client = new SftpClient(connectionInfo);
            try
            {
                client.Connect();
            }
            catch
            {
                client.Dispose();
                throw;
            }
            return client;
        }

        public void Disconnect(SftpClient client)
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
            client.Dispose();
        }
    }
}
