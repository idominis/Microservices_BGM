using System.Xml.Serialization;

namespace FrontendService.DTO
{
    public class PurchaseOrderHeaders
    {
        [XmlElement("PurchaseOrderHeader")]
        public List<PurchaseOrderHeaderDto> Headers { get; set; }
    }
}
