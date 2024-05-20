using System.Xml.Serialization;

namespace OrderManagementService.DTO
{
    [XmlRoot("ArrayOfPurchaseOrderSummary")]
    public class PurchaseOrderSummaries
    {
        [XmlElement("PurchaseOrderSummary")]
        public List<PurchaseOrderSummaryDto> Summaries { get; set; }
    }
}
