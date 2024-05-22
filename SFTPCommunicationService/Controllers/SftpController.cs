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
        public async Task<IActionResult> UploadFile([FromBody] FileUploadRequestDto request)
        {
            await _fileHandler.UploadFileAsync(request.LocalFilePath, request.RemotePath);
            return Ok("File uploaded successfully.");
        }


        [HttpPost("download-file")]
        public async Task<IActionResult> DownloadFile([FromBody] FileDownloadRequestDto request)
        {
            await _fileHandler.DownloadFileAsync(request.RemoteFilePath, request.LocalDirectory);
            return Ok("File downloaded successfully.");
        }
    }
}
