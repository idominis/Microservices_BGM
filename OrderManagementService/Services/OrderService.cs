using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrderManagementService.DTO;

namespace OrderManagementService.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _fileManagementServiceClient;
        private readonly HttpClient _sftpCommunicationServiceClient;
        private readonly HttpClient _dataAccessServiceClient;
        private readonly ILogger<OrderService> _logger;

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
            var response = await _fileManagementServiceClient.PostAsync("api/file/generate", null);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("XML generated successfully!");
                return true;
            }
            else
            {
                _logger.LogError("Failed to generate XML. StatusCode: {StatusCode}", response.StatusCode);
                return false;
            }
        }

        public async Task<bool> SendXmlAsync()
        {
            var response = await _sftpCommunicationServiceClient.PostAsync("api/sftp/upload", null);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("XML sent successfully!");
                return true;
            }
            else
            {
                _logger.LogError("Failed to send XML. StatusCode: {StatusCode}", response.StatusCode);
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
