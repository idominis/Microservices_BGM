using FileManagementService.Models;
using System;
using System.Collections.Generic;

namespace FileManagementService.Interfaces
{
    public interface IXmlService
    {
        object LoadFromXml(string filePath, string typeName);
        void GenerateXMLFiles(List<PurchaseOrderSummary> summaries, DateTime? startDate = null, DateTime? endDate = null);
        List<int> ExtractPurchaseOrderIdsFromXml(string filePath);
        List<int> ExtractPurchaseOrderDetailIdsFromXml(string filePath);
    }
}
