using Microsoft.AspNetCore.Mvc;
using SFTPCommunicationService.Services;
using System.Collections.Generic;
using Renci.SshNet.Sftp;

namespace SFTPCommunicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SftpController : ControllerBase
    {
        private readonly SftpFileHandler _fileHandler;

        public SftpController(SftpFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        [HttpGet("list-files")]
        public ActionResult<IEnumerable<SftpFile>> ListFiles(string remoteDirectory)
        {
            var files = _fileHandler.ListFiles(remoteDirectory);
            return Ok(files);
        }

        [HttpPost("upload-file")]
        public IActionResult UploadFile([FromQuery] string localFilePath, [FromQuery] string remoteDirectory)
        {
            _fileHandler.UploadFile(localFilePath, remoteDirectory);
            return Ok("File uploaded successfully.");
        }

        [HttpPost("download-file")]
        public IActionResult DownloadFile([FromQuery] string remoteFilePath, [FromQuery] string localDirectory)
        {
            _fileHandler.DownloadFile(remoteFilePath, localDirectory);
            return Ok("File downloaded successfully.");
        }
    }
}
