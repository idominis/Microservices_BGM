using System.Xml.Serialization;

namespace FileManagementService.Models
{
    [XmlRoot("PurchaseOrderDetails")]
    public class PurchaseOrderDetails
    {
        [XmlElement("PurchaseOrderDetail")]
        public List<PurchaseOrderDetail> Details { get; set; }
    }
}
