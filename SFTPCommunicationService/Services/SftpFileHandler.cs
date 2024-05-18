using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;

namespace SFTPCommunicationService.Services
{
    public class SftpFileHandler
    {
        private readonly SftpClientManager _clientManager;

        public SftpFileHandler(SftpClientManager clientManager)
        {
            _clientManager = clientManager;
        }

        public IEnumerable<SftpFile> ListFiles(string remoteDirectory)
        {
            using (var client = _clientManager.Connect())
            {
                return client.ListDirectory(remoteDirectory);
            }
        }

        public void UploadFile(string localFilePath, string remoteDirectory)
        {
            using (var client = _clientManager.Connect())
            {
                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    client.UploadFile(fileStream, Path.Combine(remoteDirectory, Path.GetFileName(localFilePath)));
                }
            }
        }

        public void DownloadFile(string remoteFilePath, string localDirectory)
        {
            using (var client = _clientManager.Connect())
            {
                string localFilePath = Path.Combine(localDirectory, Path.GetFileName(remoteFilePath));
                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    client.DownloadFile(remoteFilePath, fileStream);
                }
            }
        }
    }
}
