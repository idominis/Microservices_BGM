using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace SFTPCommunicationService.Services
{
    public class SftpClientManager
    {
        private readonly ConnectionInfo _connectionInfo;

        public SftpClientManager(string host, string username, string password)
        {
            _connectionInfo = new ConnectionInfo(host, username, new PasswordAuthenticationMethod(username, password));
        }

        public SftpClient Connect()
        {
            var client = new SftpClient(_connectionInfo);
            try
            {
                client.Connect();
            }
            catch
            {
                client.Dispose(); // Ensure disposal if connection fails
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
