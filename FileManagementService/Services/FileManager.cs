﻿using System;
using System.IO;

namespace FileManagementService.Services
{
    public class FileManager
    {
        private readonly string _baseDirectoryPath;
        private readonly string _remoteDetailsDirectoryPath;
        private readonly string _remoteHeadersDirectoryPath;
        private readonly string _baseDirectoryXmlCreatedPath;
        private readonly string _remoteDirectoryPath;

        public FileManager()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _baseDirectoryPath = Path.Combine(documentsPath, "BGM_project", "local", "data_received");
            _baseDirectoryXmlCreatedPath = Path.Combine(documentsPath, "BGM_project", "local", "XML_created");
            _remoteDetailsDirectoryPath = @"\purchasingOrdersDetails";
            _remoteHeadersDirectoryPath = @"\purchasingOrdersHeaders";
            _remoteDirectoryPath = @"\";
            Directory.CreateDirectory(_baseDirectoryPath);
        }

        public string GetBaseDirectoryPath()
        {
            return _baseDirectoryPath;
        }

        public string GetBaseDirectoryXmlCreatedPath()
        {
            return _baseDirectoryXmlCreatedPath;
        }

        public string GetRemoteDetailsDirectoryPath()
        {
            return _remoteDetailsDirectoryPath;
        }

        public string GetRemoteHeadersDirectoryPath()
        {
            return _remoteHeadersDirectoryPath;
        }

        public string GetSpecificLocalPath(string subDirectory)
        {
            var fullPath = Path.Combine(_baseDirectoryPath, subDirectory);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        public string GetSpecificRemotePath(string subDirectory)
        {
            var fullPath = Path.Combine(_remoteDirectoryPath, subDirectory);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }
}
