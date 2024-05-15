using FileManagementService.Interfaces;
using FileManagementService.Models;
//using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FileManagementService.Services
{
    public class XmlService : IXmlService
    {
        private readonly FileManager _fileManager;

        public XmlService(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public object LoadFromXml(string filePath, string typeName)
        {
            string localBaseDirectoryPath = _fileManager.GetBaseDirectoryPath();
            IEnumerable<string> directories = Directory.GetDirectories(localBaseDirectoryPath, "*", SearchOption.AllDirectories);

            foreach (string dir in directories)
            {
                var xmlFiles = Directory.GetFiles(dir, "*.xml");
                if (dir.EndsWith("headers")) continue;  // Skip the headers directory
            }

            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException($"Type '{typeName}' not found.");
            }

            XmlSchemaSet schemas = new XmlSchemaSet();
            //schemas.Add("", "path_to_your_xsd_file"); // Update the schema path as needed

            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            };
            settings.ValidationEventHandler += ValidationEventHandler;

            try
            {
                using (var stream = File.OpenRead(filePath))
                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    while (reader.Read()) { }
                }

                using (var stream = File.OpenRead(filePath))
                {
                    var serializer = new XmlSerializer(type);
                    return serializer.Deserialize(stream);
                }
            }
            catch (XmlException xmlEx)
            {
                //Log.Error($"XML format error in {filePath}: {xmlEx.Message}");
                return null;
            }
            catch (InvalidOperationException ex) when (ex.InnerException is FormatException)
            {
                //Log.Error($"Schema validation error in {filePath}: {ex.InnerException.Message}");
                return null;
            }
            catch (Exception ex)
            {
                //Log.Error($"General error processing file {filePath}: {ex.Message}");
                return null;
            }
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error || e.Severity == XmlSeverityType.Warning)
            {
                //Log.Warning($"Validation warning/error: {e.Message}");
            }
        }

        public void GenerateXMLFiles(List<PurchaseOrderSummary> summaries, DateTime? startDate = null, DateTime? endDate = null)
        {
            var serializer = new XmlSerializer(typeof(List<PurchaseOrderSummary>));
            string localBaseDirectoryXmlCreatedPath = _fileManager.GetBaseDirectoryXmlCreatedPath();
            Directory.CreateDirectory(localBaseDirectoryXmlCreatedPath);

            if (startDate.HasValue && endDate.HasValue)
            {
                string dateRangeFileName = Path.Combine(localBaseDirectoryXmlCreatedPath, $"PurchaseOrderSummariesGenerated_{startDate.Value:yyyyMMdd}_to_{endDate.Value:yyyyMMdd}.xml");
                using (var stream = new FileStream(dateRangeFileName, FileMode.Create))
                {
                    serializer.Serialize(stream, summaries);
                }
            }
            else
            {
                foreach (var group in summaries.GroupBy(s => s.PurchaseOrderID))
                {
                    string fileName = Path.Combine(localBaseDirectoryXmlCreatedPath, $"PurchaseOrderGenerated_{group.Key}.xml");
                    using (var stream = new FileStream(fileName, FileMode.Create))
                    {
                        serializer.Serialize(stream, group.ToList());
                    }
                }
            }
        }

        public List<int> ExtractPurchaseOrderIdsFromXml(string filePath)
        {
            try
            {
                var summaries = (PurchaseOrderSummaries)LoadFromXml(filePath, typeof(PurchaseOrderSummaries).FullName);
                return summaries.Summaries.Select(s => s.PurchaseOrderID).Distinct().ToList();
            }
            catch (ApplicationException ex)
            {
                throw new ApplicationException("Failed to extract Purchase Order IDs.", ex);
            }
        }

        public List<int> ExtractPurchaseOrderDetailIdsFromXml(string filePath)
        {
            try
            {
                var summaries = (PurchaseOrderSummaries)LoadFromXml(filePath, typeof(PurchaseOrderSummaries).FullName);
                return summaries.Summaries.Select(s => s.PurchaseOrderDetailID).Distinct().ToList();
            }
            catch (ApplicationException ex)
            {
                throw new ApplicationException("Failed to extract Purchase Order Detail IDs.", ex);
            }
        }
    }
}
