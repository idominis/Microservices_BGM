using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly HttpClient _frontendServiceClient;
        public event Action<DateTime> LatestDateUpdated;

        public OrderService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OrderService> logger)
        {
            _fileManagementServiceClient = httpClientFactory.CreateClient("FileManagementServiceClient");
            _sftpCommunicationServiceClient = httpClientFactory.CreateClient("SFTPCommunicationServiceClient");
            _dataAccessServiceClient = httpClientFactory.CreateClient("DataAccessServiceClient");
            _frontendServiceClient = httpClientFactory.CreateClient("FrontendServiceClient");
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

        public async Task<bool> GenerateXmlAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var responseAlreadyGeneratedSummaries = await _dataAccessServiceClient.GetAsync("api/data/fetch-generated");

            if (!responseAlreadyGeneratedSummaries.IsSuccessStatusCode)
            {
                // Handle the error
                var error = await responseAlreadyGeneratedSummaries.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch generated summaries: {error}");
            }

            var resultAlreadyGeneratedSummaries = await responseAlreadyGeneratedSummaries.Content.ReadFromJsonAsync<HashSet<int>>();

            var fetchSummariesRequest = new FetchSummariesRequestDto
            {
                AlreadyGeneratedIds = resultAlreadyGeneratedSummaries,
                StartDate = startDate,
                EndDate = endDate
            };

            var responseSummariesToGenerate = await _dataAccessServiceClient.PostAsJsonAsync("api/data/fetch-summaries-to-generate", fetchSummariesRequest);

            if (!responseSummariesToGenerate.IsSuccessStatusCode)
            {
                // Handle the error
                var error = await responseSummariesToGenerate.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch summaries to generate: {error}");
            }

            var summariesToGenerate = await responseSummariesToGenerate.Content.ReadFromJsonAsync<List<PurchaseOrderSummaryDto>>();

            // Proceed with generating XML for summariesToGenerate
            if (summariesToGenerate == null || summariesToGenerate.Count == 0)
            {
                Log.Information("No summaries to generate XML for.");
                return true;
            }

            var responseResult = await _fileManagementServiceClient.PostAsJsonAsync("api/FileManagementService/generate-xml", summariesToGenerate);

            if (responseResult.IsSuccessStatusCode)
            {
                // Loop through the summaries and create/upload the corresponding XML files
                foreach (var summary in summariesToGenerate)
                {
                    var responseUpdateStatus = await _dataAccessServiceClient.PutAsync($"api/data/update-po-status/{summary.PurchaseOrderID}/{summary.PurchaseOrderDetailID}/{true}/{false}/{0}", null); // Mark as processed, not sent, base channel
                    if (responseUpdateStatus.IsSuccessStatusCode)
                    {
                        Log.Information($"PurchaseOrderId: {summary.PurchaseOrderID} with PurchaseOrderDetailId: {summary.PurchaseOrderDetailID} created");
                    }
                    else
                    {
                        Log.Information($"PurchaseOrderId: {summary.PurchaseOrderID} with PurchaseOrderDetailId: {summary.PurchaseOrderDetailID} failed to create");
                    }
                }

                _logger.LogInformation("XML generated successfully!");
                return true;
            }
            else
            {
                _logger.LogError("Failed to generate XML. StatusCode: {StatusCode}", responseResult.StatusCode);
                return false;
            }
        }


        public async Task<bool> SendXmlAsync()
        {
            try
            {
                string baseDirectoryXmlCreatedPath = await GetPathAsync("baseDirectoryXmlCreatedPath");
                string remoteDirectoryPath = await GetPathAsync("remoteDirectoryPath");
                var alreadySentIds = new HashSet<int>();
                DateTime? latestDate = null;

                var responseAlreadySent = await _dataAccessServiceClient.GetAsync("api/data/fetch-sent");
                if (!responseAlreadySent.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to fetch already sent IDs: {responseAlreadySent.StatusCode}");
                    return false;
                }

                alreadySentIds = await responseAlreadySent.Content.ReadFromJsonAsync<HashSet<int>>();

                var responseGenerated = await _dataAccessServiceClient.GetAsync("api/data/fetch-generated");
                if (!responseGenerated.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to fetch generated IDs: {responseGenerated.StatusCode}");
                    return false;
                }

                var resultGeneratedIds = await responseGenerated.Content.ReadFromJsonAsync<HashSet<int>>();

                foreach (var orderId in resultGeneratedIds)
                {
                    string fileName = $"PurchaseOrderGenerated_{orderId}.xml";
                    string filePath = Path.Combine(baseDirectoryXmlCreatedPath, fileName);

                    if (!File.Exists(filePath))
                    {
                        Log.Warning($"File not found: {filePath}");
                        continue;
                    }

                    var responseOrderIdsFromXml = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/extract-purchase-order-id?filePath={Uri.EscapeDataString(filePath)}");
                    if (!responseOrderIdsFromXml.IsSuccessStatusCode)
                    {
                        Log.Error($"Failed to extract purchase order IDs from: {filePath}");
                        continue;
                    }

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

                        if (!responseUpload.IsSuccessStatusCode)
                        {
                            Log.Error($"Failed to upload file: {filePath}");
                            continue; // Move to the next file if upload fails
                        }

                        Log.Information($"File uploaded successfully: {filePath}");

                        var responseDetailIds = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/extract-purchase-order-detail-id?filePath={Uri.EscapeDataString(filePath)}");
                        if (!responseDetailIds.IsSuccessStatusCode)
                        {
                            Log.Error($"Failed to extract purchase order detail IDs from: {filePath}");
                            continue;
                        }

                        var resultDetailIds = await responseDetailIds.Content.ReadFromJsonAsync<List<int>>();

                        foreach (var detailsId in resultDetailIds)
                        {
                            var responseUpdateStatus = await _dataAccessServiceClient.PutAsync($"api/data/update-po-status/{id}/{detailsId}/{true}/{true}/{0}", null);
                            if (responseUpdateStatus.IsSuccessStatusCode)
                            {
                                Log.Information($"PurchaseOrderId: {id} with PurchaseOrderDetailId: {detailsId} sent");
                            }
                            else
                            {
                                Log.Error($"Failed to update status for PurchaseOrderDetailId: {detailsId}");
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

                                // Send the latest date update to clients via FrontendService
                                var notifyResponse = await _frontendServiceClient.PostAsJsonAsync("api/frontend/notify-latest-date", resultFileDate.Value);
                                if (!notifyResponse.IsSuccessStatusCode)
                                {
                                    Log.Error($"Failed to notify frontend about latest date update: {notifyResponse.StatusCode}");
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


        public async Task<bool> SendDateGeneratedXmlAsync(DateTime startDate, DateTime endDate)
        {
            // Path where the XML files are generated and will be uploaded from
            string baseDirectoryXmlCreatedPath = await GetPathAsync("baseDirectoryXmlCreatedPath");

            try
            {
                var dateRange = new DateRangeDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Retrieve all purchase order summaries within the specified date range
                var responseSummariesByDate = await _dataAccessServiceClient.PostAsJsonAsync("api/data/fetch-summaries-date", dateRange);
                if (!responseSummariesByDate.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to fetch summaries by date: {responseSummariesByDate.StatusCode}");
                    return false;
                }

                var purchaseOrderSummariesByDate = await responseSummariesByDate.Content.ReadFromJsonAsync<List<PurchaseOrderSummaryDto>>();

                var responseAlreadySentIds = await _dataAccessServiceClient.GetAsync("api/data/fetch-sent");
                if (!responseAlreadySentIds.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to fetch already sent IDs: {responseAlreadySentIds.StatusCode}");
                    return false;
                }

                var alreadySentIds = await responseAlreadySentIds.Content.ReadFromJsonAsync<HashSet<int>>();

                // Filter out already sent purchase orders to get only those that need to be uploaded
                List<PurchaseOrderSummaryDto> idsToUpload = purchaseOrderSummariesByDate
                    .Where(summary => !alreadySentIds.Contains(summary.PurchaseOrderID))
                    .ToList();

                if (!idsToUpload.Any())
                {
                    Log.Error("All Purchase Orders in the specified date range have already been sent.");
                    return false;
                }

                // Generate the XML files for the filtered summaries
                var responseIdsToUpload = await _fileManagementServiceClient.PostAsJsonAsync("api/FileManagementService/generate-xml", idsToUpload);
                if (!responseIdsToUpload.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to generate XML files: {responseIdsToUpload.StatusCode}");
                    return false;
                }

                // Loop through the summaries and create/upload the corresponding XML files
                foreach (var summary in idsToUpload)
                {
                    // Create the file name dynamically using the PurchaseOrderID
                    string fileName = $"PurchaseOrderGenerated_{summary.PurchaseOrderID}.xml";
                    string filePath = Path.Combine(baseDirectoryXmlCreatedPath, fileName);

                    // Determine the remote directory path where the file will be uploaded
                    string remoteDirectoryPath = await GetPathAsync("remoteDirectoryPath");
                    string remotePath = Path.Combine(remoteDirectoryPath, fileName);

                    // Upload the file to the remote path
                    var responseUpload = await _sftpCommunicationServiceClient.PostAsJsonAsync("api/SFTP/upload-file", new FileUploadRequestDto { LocalFilePath = filePath, RemotePath = remotePath });
                    if (!responseUpload.IsSuccessStatusCode)
                    {
                        Log.Error($"Failed to upload file: {filePath}");
                        continue;
                    }

                    // Extract the detail IDs from the generated XML file & Update the database with the upload status for each detail
                    var responseDetailIds = await _fileManagementServiceClient.GetAsync($"api/FileManagementService/extract-purchase-order-detail-id?filePath={Uri.EscapeDataString(filePath)}");
                    if (!responseDetailIds.IsSuccessStatusCode)
                    {
                        Log.Error($"Failed to extract purchase order detail IDs from: {filePath}");
                        continue;
                    }

                    var resultDetailIds = await responseDetailIds.Content.ReadFromJsonAsync<List<int>>();

                    foreach (var detailsId in resultDetailIds)
                    {
                        var responseUpdateStatus = await _dataAccessServiceClient.PutAsync($"api/data/update-po-status/{summary.PurchaseOrderID}/{detailsId}/{true}/{true}/{0}", null);
                        if (responseUpdateStatus.IsSuccessStatusCode)
                        {
                            Log.Information($"PurchaseOrderId: {summary.PurchaseOrderID} with PurchaseOrderDetailId: {detailsId} sent");
                            Log.Information($"PurchaseOrderDetailId sent: {detailsId}");
                        }
                        else
                        {
                            Log.Error($"Failed to update status for PurchaseOrderDetailId: {detailsId}");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send Purchase Order XML files for the specified date range.");
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
