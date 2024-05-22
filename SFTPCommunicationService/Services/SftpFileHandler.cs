using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Collections.Generic;
using System.IO;
using Serilog;

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

        public async Task UploadFileAsync(string localFilePath, string remotePath)
        {
            try
            {
                using (var client = _clientManager.Connect())
                {
                    string remoteDirectory = Path.GetDirectoryName(remotePath);

                    if (!client.Exists(remoteDirectory))
                    {
                        client.CreateDirectory(remoteDirectory);
                    }

                    using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await Task.Run(() => client.UploadFile(fileStream, remotePath));
                    }
                    _clientManager.Disconnect(client);
                }
                Log.Information("File uploaded successfully: {LocalFilePath}", localFilePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upload file: {LocalFilePath}", localFilePath);
                throw;
            }
        }

        public async Task<bool> DownloadFileAsync(string remoteFilePath, string localDirectory)
        {
            bool newFilesDownloaded = false;

            try
            {
                using (var client = _clientManager.Connect())
                {
                    var entries = client.ListDirectory(remoteFilePath);

                    if (!Directory.Exists(localDirectory))
                    {
                        Directory.CreateDirectory(localDirectory);
                    }

                    foreach (var entry in entries)
                    {
                        if (entry.IsDirectory && entry.Name != "." && entry.Name != "..")
                        {
                            string subDirectoryPath = Path.Combine(remoteFilePath, entry.Name);
                            string localSubDirectoryPath = Path.Combine(localDirectory, entry.Name);

                            if (!Directory.Exists(localSubDirectoryPath))
                            {
                                Directory.CreateDirectory(localSubDirectoryPath);
                            }

                            newFilesDownloaded |=  await DownloadFileAsync(subDirectoryPath, localSubDirectoryPath);
                        }
                        else if (!entry.IsDirectory && !entry.Name.EndsWith(".processed"))
                        {
                            SftpFile file = entry as SftpFile;
                            if (file != null)
                            {
                                string localFilePath = Path.Combine(localDirectory, entry.Name);

                                bool fileExists = File.Exists(localFilePath);
                                bool sizeMatches = fileExists && new FileInfo(localFilePath).Length == entry.Attributes.Size;

                                if (!fileExists || !sizeMatches)
                                {
                                    newFilesDownloaded |= ProcessFilesInDirectory(client, new[] { file }, localDirectory);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to download XML files from directory: {DirectoryPath}", remoteFilePath);
                throw;
            }

            return newFilesDownloaded;
        }

        private bool ProcessFilesInDirectory(SftpClient client, IEnumerable<SftpFile> files, string localDirectory)
        {
            bool downloaded = false;
            foreach (var file in files)
            {
                string localFilePath = Path.Combine(localDirectory, file.Name);
                if (!File.Exists(localFilePath))
                {
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        client.DownloadFile(file.FullName, fileStream);
                    }
                    downloaded = true;

                    string processedFilePath = file.FullName + ".processed";
                    client.RenameFile(file.FullName, processedFilePath);
                    Log.Information("File {FileName} downloaded and marked as processed.", file.Name);
                }
            }
            return downloaded;
        }

    }
}
