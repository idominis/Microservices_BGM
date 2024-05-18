using System.Xml.Serialization;

namespace FileManagementService.Models
{
    public class PurchaseOrderDetail
    {
        [XmlAttribute]
        public int PurchaseOrderID { get; set; }

        [XmlAttribute]
        public int PurchaseOrderDetailID { get; set; }

        [XmlElement]
        public DateTime DueDate { get; set; }

        [XmlElement]
        public int OrderQty { get; set; }

        [XmlElement]
        public int ProductID { get; set; }

        [XmlElement]
        public decimal UnitPrice { get; set; }

        [XmlElement]
        public decimal LineTotal { get; set; }

        [XmlElement]
        public decimal ReceivedQty { get; set; }

        [XmlElement]
        public decimal RejectedQty { get; set; }

        [XmlElement]
        public decimal StockedQty { get; set; }

        [XmlElement]
        public DateTime ModifiedDate { get; set; }
    }
}
