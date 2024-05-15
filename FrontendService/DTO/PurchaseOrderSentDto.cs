using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FrontendService.DTO
{
    public partial class PurchaseOrderSentDto
    {
        public int PurchaseOrderId { get; set; }
        public int PurchaseOrderDetailId { get; set; }
        public bool OrderProcessed { get; set; }
        public bool OrderSent { get; set; }
        public int Channel { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}