using System.Xml.Serialization;

namespace FileManagementService.Models
{
    public class PurchaseOrderHeaders
    {
        [XmlElement("PurchaseOrderHeader")]
        public List<PurchaseOrderHeaderDto> Headers { get; set; }
    }
}
