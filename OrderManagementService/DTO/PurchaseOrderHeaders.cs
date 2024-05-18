using System.Xml.Serialization;

namespace OrderManagementService.DTO
{
    public class PurchaseOrderHeaders
    {
        [XmlElement("PurchaseOrderHeader")]
        public List<PurchaseOrderHeaderDto> Headers { get; set; }
    }
}
