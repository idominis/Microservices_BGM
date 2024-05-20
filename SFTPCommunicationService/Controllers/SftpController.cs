using Microsoft.AspNetCore.Mvc;
using SFTPCommunicationService.Services;
using System.Collections.Generic;
using Renci.SshNet.Sftp;
using SFTPCommunicationService.DTO;

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
            _fileHandler.UploadFileAsync(localFilePath, remoteDirectory);
            return Ok("File uploaded successfully.");
        }

        [HttpPost("download-file")]
        public async Task<IActionResult> DownloadFile([FromBody] FileDownloadRequestDto request)
        {
            await _fileHandler.DownloadFile(request.RemoteFilePath, request.LocalDirectory);
            return Ok("File downloaded successfully.");
        }
    }
}
