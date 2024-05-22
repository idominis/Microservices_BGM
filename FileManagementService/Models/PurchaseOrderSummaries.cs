using System.Xml.Serialization;

namespace FileManagementService.Models
{
    [XmlRoot("ArrayOfPurchaseOrderSummary")]
    public class PurchaseOrderSummaries
    {
        [XmlElement("PurchaseOrderSummary")]
        public List<PurchaseOrderSummary> Summaries { get; set; }
    }
}
