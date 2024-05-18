using System.Xml.Serialization;

namespace OrderManagementService.DTO
{
    public class PurchaseOrderDetails
    {
        [XmlElement("PurchaseOrderDetail")]
        public List<PurchaseOrderDetailDto> Details { get; set; }
    }
}
