namespace SFTPCommunicationService.DTO
{
    public class FileUploadRequestDto
    {
        public string LocalFilePath { get; set; }
        public string RemotePath { get; set; }
    }
}
