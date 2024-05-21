using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using OrderManagementService.DTO;
using Serilog;

namespace OrderManagementService.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _fileManagementServiceClient;
        private readonly HttpClient _sftpCommunicationServiceClient;
        private readonly HttpClient _dataAccessServiceClient;
        private readonly ILogger<OrderService> _logger;
        public event Action<DateTime> LatestDateUpdated;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OrderService> logger)
        {
            _fileManagementServiceClient = httpClientFactory.CreateClient("FileManagementServiceClient");
            _sftpCommunicationServiceClient = httpClientFactory.CreateClient("SFTPCommunicationServiceClient");
            _dataAccessServiceClient = httpClientFactory.CreateClient("DataAccessServiceClient");
            _logger = logger;
        }

        public async Task<string> GetPathAsync(string pathName)
        {
            var response = await _fileManagementServiceClient.GetAsync("api/FileManagementService/get-file-paths");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var paths = JObject.Parse(result);

            if (paths[pathName] != null)
            {
                return paths[pathName].ToString();
            }

            throw new KeyNotFoundException($"Path '{pathName}' not found in the response.");
        }

        public async Task<bool> DownloadFilesPODAsync()
        {
            string localBaseDirectoryPath = await GetPathAsync("baseDirectoryPath");
            string remoteDetailsDirectoryPath = await GetPathAsync("remoteDetailsDirectoryPath");

            var fileDownloadRequest = new FileDownloadRequestDto
            {
                RemoteFilePath = remoteDetailsDirectoryPath,
                LocalDirectory = localBaseDirectoryPath
            };

            bool shouldRetry = true;

            while (shouldRetry)
            {
                try
                {
                    var response = await _sftpCommunicationServiceClient.PostAsJsonAsync("api/SFTP/download-file", fileDownloadRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        //_statusUpdateService.RaiseStatusUpdated($"XML files from {remotePath} have been downloaded successfully!");
                        Log.Information($"XML files from {fileDownloadRequest.RemoteFilePath} have been downloaded successfully!");
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        Log.Information($"Server error.");
                    }
                    else
                    {
                        //_statusUpdateService.RaiseStatusUpdated($"No new XML files in {remotePath}.");
                        Log.Information($"No new XML files in {fileDownloadRequest.RemoteFilePath}.");
                    }

                    shouldRetry = false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> DownloadFilesPOHAsync()
        {
            string localBaseDirectoryPath = await GetPathAsync("baseHeadersDirectoryPath");
            string remoteHeadersDirectoryPath = await GetPathAsync("remoteHeadersDirectoryPath");

            var fileDownloadRequest = new FileDownloadRequestDto
            {
                RemoteFilePath = remoteHeadersDirectoryPath,
                LocalDirectory = localBaseDirectoryPath
            };

            bool shouldRetry = true;

            while (shouldRetry)
            {
                try
                {
                    var response = await _sftpCommunicationServiceClient.PostAsJsonAsync("api/SFTP/download-file", fileDownloadRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"XML files from {fileDownloadRequest.RemoteFilePath} have been downloaded successfully!");
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        Log.Information($"Server error.");
                    }
                    else
                    {
                        Log.Information($"No new XML files in {fileDownloadRequest.RemoteFilePath}.");
                    }
                    shouldRetry = false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return false;
        }

        public async Task<bool> SavePODetailsAsync(List<PurchaseOrderDetailDto> purchaseOrderDetailsDto)
        {
            //var podDetails = await FetchPODetailsAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-pod", purchaseOrderDetailsDto);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("POD saved successfully!");
                return true;
            }
            else
            {
                _logger.LogError("Failed to save POD. StatusCode: {StatusCode}", response.StatusCode);
                return false;
            }
        }

        public async Task<bool> SavePOHeadersAsync(List<PurchaseOrderHeaderDto> purchaseOrderHeadersDto)
        {
            //var pohHeaders = await FetchPOHeadersAsync();
            var response = await _dataAccessServiceClient.PostAsJsonAsync("api/data/save-poh", purchaseOrderHeadersDto);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("POH saved successfully!");
                return true;
            }
            else
            {
                _logger.LogError("Failed to save POH. StatusCode: {StatusCode}", response.StatusCode);
                return false;
            }
        }

        public async Task<bool> GenerateXmlAsync()
        {
            var responseSummaries = await _dataAccessServiceClient.GetAsync("api/data/fetch-summaries");

            if(responseSummaries.IsSuccessStatusCode)
            {
                var result = await responseSummaries.Content.ReadFromJsonAsync<List<PurchaseOrderSummaryDto>>();

                var responseResult = await _fileManagementServiceClient.PostAsJsonAsync("api/FileManagementService/generate-xml", result);

                if (responseResult.IsSuccessStatusCode)
                {
                    _logger.LogInformation("XML generated successfully!");
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to generate XML. StatusCode: {StatusCode}", responseResult.StatusCode);
                    return false;
                }
            }

            return false;


        }

        public async Task<bool> SendXmlAsync()
        {
            try
            {
                string localBaseDirectoryPath = await GetPathAsync("baseDirectoryPath");
                string remoteDirectoryPath = await GetPathAsync("remoteDirectoryPath");
                var alreadySentIds = new HashSet<int>();
                DateTime? latestDate = null;

                var responseAlreadySent = await _dataAccessServiceClient.GetAsync("api/data/fetch-sent");

                if (responseAlreadySent.IsSuccessStatusCode)
                {
                    var resultAlreadySent = await responseAlreadySent.Content.ReadFromJsonAsync<HashSet<int>>();
                    alreadySentIds = resultAlreadySent;
                }

                var responseGenerated = await _dataAccessServiceClient.GetAsync("api/data/fetch-generated");

                if (responseGenerated.IsSuccessStatusCode)
                {
                    var resultGeneratedIds = await responseGenerated.Content.ReadFromJsonAsync<HashSet<int>>();

                    foreach (var orderId in resultGeneratedIds)
                    {
                        string fileName = $"PurchaseOrderGenerated_{orderId}.xml";
                        string filePath = Path.Combine(localBaseDirectoryPath, fileName);

                        if (!File.Exists(filePath))
                        {
                            Log.Warning($"File not found: {filePath}");
                            continue;
                        }

                        var responseOrderIdsFromXml = await _fileManagementServiceClient.PostAsJsonAsync("api/FileManagementService/extract-purchase-order-ids", filePath);

                        if (responseOrderIdsFromXml.IsSuccessStatusCode)
                        {
                            var resultOrderIdsFromXml = await responseOrderIdsFromXml.Content.ReadFromJsonAsync<List<int>>();

                            foreach (var id in resultOrderIdsFromXml)
                            {
                                if (alreadySentIds.Contains(id))
                                {
                                    Log.Information($"PurchaseOrderId already sent: {id}");
                                    continue;
                                }

                                string remoteFilePath = Path.Combine(remoteDirectoryPath, fileName);
                                var responseUpload = await _sftpCommunicationServiceClient.PostAsJsonAsync("api/SFTP/upload-file", new FileUploadRequestDto { LocalFilePath = filePath, RemotePath = remoteFilePath });

                                if (responseUpload.IsSuccessStatusCode)
                                {
                                    Log.Information($"File uploaded successfully: {filePath}");
                                }
                                else
                                {
                                    Log.Error($"Failed to upload file: {filePath}");
                                    return false; // Exit on upload failure
                                }

                                // Update the status of the purchase order (have to update the status of each detail)
                                var responseDetailIds = await _fileManagementServiceClient.PostAsJsonAsync("api/FileManagementService/extract-purchase-order-detail-ids", filePath);

                                if (responseDetailIds.IsSuccessStatusCode)
                                {
                                    var resultDetailIds = await responseDetailIds.Content.ReadFromJsonAsync<List<int>>();

                                    foreach (var detailsId in resultDetailIds)
                                    {
                                        var responseUpdateStatus = await _dataAccessServiceClient.PutAsync($"api/data/update-status/{id}/{detailsId}/{true}/{true}/{0}", null);

                                        if (responseUpdateStatus.IsSuccessStatusCode)
                                        {
                                            Log.Information($"PurchaseOrderDetailId sent: {detailsId}");
                                        }
                                        else
                                        {
                                            Log.Error($"Failed to update status for PurchaseOrderDetailId: {detailsId}");
                                        }
                                    }
                                }

                                var responseFileDate = await _dataAccessServiceClient.GetAsync($"api/data/get-po-latest-date/{orderId}");

                                if (responseFileDate.IsSuccessStatusCode)
                                {
                                    var resultFileDate = await responseFileDate.Content.ReadFromJsonAsync<DateTime?>();

                                    if (resultFileDate.HasValue && (latestDate == null || resultFileDate > latestDate))
                                    {
                                        latestDate = resultFileDate;
                                        LatestDateUpdated?.Invoke(resultFileDate.Value);
                                    }
                                }

                            }
                        }
                    }
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "HTTP request failed.");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred.");
                return false;
            }
        }


        public async Task<List<PurchaseOrderDetailDto>> FetchPODetailsAsync()
        {
            List<PurchaseOrderDetailDto> allPurchaseOrderDetails = new List<PurchaseOrderDetailDto>();
            string localBaseDirectoryPath = await GetPathAsync("baseDirectoryPath");
            string invalidDataDirectoryPath = Path.Combine(localBaseDirectoryPath, "data_received_invalid");
            Directory.CreateDirectory(invalidDataDirectoryPath);  // Ensure the invalid directory exists
            IEnumerable<string> directories = Directory.GetDirectories(localBaseDirectoryPath, "*", SearchOption.AllDirectories);
   
            foreach (string dir in directories)
            {
                if (dir.EndsWith("headers")) continue;  // Skip the headers directory

                var xmlFiles = Directory.GetFiles(dir, "*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        var typeName = "FileManagementService.Models.PurchaseOrderDetails";
                        var response = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/load-xml?filePath={Uri.EscapeDataString(xmlFile)}&typeName={Uri.EscapeDataString(typeName)}");

                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadFromJsonAsync<PurchaseOrderDetails>();
                            if (result != null)
                            {
                                foreach (var detail in result.Details)
                                {
                                    allPurchaseOrderDetails.Add(new PurchaseOrderDetailDto
                                    {
                                        PurchaseOrderDetailId = detail.PurchaseOrderDetailId,
                                        PurchaseOrderId = detail.PurchaseOrderId,
                                        DueDate = detail.DueDate,
                                        OrderQty = detail.OrderQty,
                                        ProductId = detail.ProductId,
                                        UnitPrice = detail.UnitPrice,
                                        LineTotal = detail.LineTotal,
                                        ReceivedQty = detail.ReceivedQty,
                                        RejectedQty = detail.RejectedQty,
                                        StockedQty = detail.StockedQty,
                                        ModifiedDate = detail.ModifiedDate
                                    });
                                }
                            }
                        }
                        //if (purchaseOrderDetails == null)
                        //{
                        //    //MoveInvalidFile(xmlFile, invalidDataDirectoryPath);
                        //    continue;
                        //}

                        bool hasInvalidEntries = false;
                        //foreach (var detail in purchaseOrderDetails.Details)
                        //{
                        //    ValidationResult results = new PurchaseOrderDetailValidator().Validate(detail);
                        //    if (!results.IsValid)
                        //    {
                        //        //Log.Information($"Validation failed for {xmlFile}. Reason: {string.Join("; ", results.Errors.Select(e => e.ErrorMessage))}");
                        //        hasInvalidEntries = true;
                        //    }
                        //    else
                        //    {
                        //        allPurchaseOrderDetails.Add(detail);
                        //    }
                        //}

                        if (hasInvalidEntries)
                        {
                            //MoveInvalidFile(xmlFile, invalidDataDirectoryPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        //_errorHandler.LogError(ex, "Error loading or processing XML data", xmlFile, "XMLProcessing");
                    }                  
                }
            }
            return allPurchaseOrderDetails;
        }

        public async Task<List<PurchaseOrderHeaderDto>> FetchPOHeadersAsync()
        {
            List<PurchaseOrderHeaderDto> allPurchaseOrderHeaders = new List<PurchaseOrderHeaderDto>();
            string localBaseDirectoryPath = await GetPathAsync("baseDirectoryPath");
            string invalidDataDirectoryPath = Path.Combine(localBaseDirectoryPath, "data_received_invalid");
            Directory.CreateDirectory(invalidDataDirectoryPath);  // Ensure the invalid directory exists
            string headersDirectoryPath = Path.Combine(localBaseDirectoryPath, "headers");

            if (Directory.Exists(headersDirectoryPath)) // Check if the headers directory exists
            {
                var xmlFiles = Directory.GetFiles(headersDirectoryPath, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        var typeName = "FileManagementService.Models.PurchaseOrderHeaders";
                        var response = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/load-xml?filePath={Uri.EscapeDataString(xmlFile)}&typeName={Uri.EscapeDataString(typeName)}");

                        var requestUrl = $"api/FileManagementService/load-xml?filePath={Uri.EscapeDataString(xmlFile)}&typeName={Uri.EscapeDataString(typeName)}";

                        // Log the request URL for debugging purposes
                        _logger.LogInformation("Request URL: {RequestUrl}", requestUrl);


                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadFromJsonAsync<PurchaseOrderHeaders>();
                            if (result != null)
                            {
                                foreach (var header in result.Headers)
                                {
                                    allPurchaseOrderHeaders.Add(new PurchaseOrderHeaderDto
                                    {
                                        PurchaseOrderId = header.PurchaseOrderId,
                                        RevisionNumber = header.RevisionNumber,
                                        Status = header.Status,
                                        EmployeeId = header.EmployeeId,
                                        VendorId = header.VendorId,
                                        ShipMethodId = header.ShipMethodId,
                                        OrderDate = header.OrderDate,
                                        ShipDate = header.ShipDate,
                                        SubTotal = header.SubTotal,
                                        TaxAmt = header.TaxAmt,
                                        Freight = header.Freight,
                                        TotalDue = header.TotalDue,
                                        ModifiedDate = header.ModifiedDate
                                    });
                                }
                            }
                        }
                        //if (purchaseOrderDetails == null)
                        //{
                        //    //MoveInvalidFile(xmlFile, invalidDataDirectoryPath);
                        //    continue;
                        //}

                        bool hasInvalidEntries = false;
                        //foreach (var detail in purchaseOrderDetails.Details)
                        //{
                        //    ValidationResult results = new PurchaseOrderDetailValidator().Validate(detail);
                        //    if (!results.IsValid)
                        //    {
                        //        //Log.Information($"Validation failed for {xmlFile}. Reason: {string.Join("; ", results.Errors.Select(e => e.ErrorMessage))}");
                        //        hasInvalidEntries = true;
                        //    }
                        //    else
                        //    {
                        //        allPurchaseOrderDetails.Add(detail);
                        //    }
                        //}

                        if (hasInvalidEntries)
                        {
                            //MoveInvalidFile(xmlFile, invalidDataDirectoryPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        //_errorHandler.LogError(ex, "Error loading or processing XML data", xmlFile, "XMLProcessing");
                    }
                }


            }


            return allPurchaseOrderHeaders;
        }
    }
}
