using System.Xml.Serialization;

namespace FrontendService.DTO
{
    public class PurchaseOrderDetails
    {
        [XmlElement("PurchaseOrderDetail")]
        public List<PurchaseOrderDetailDto> Details { get; set; }
    }
}
