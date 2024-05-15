using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendService.DTO
{
    public class PurchaseOrderSummary
    {
        public int PurchaseOrderID { get; set; }
        public int PurchaseOrderDetailID { get; set; }
        public DateTime OrderDate { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public int ProductID { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public int OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
    }

}
