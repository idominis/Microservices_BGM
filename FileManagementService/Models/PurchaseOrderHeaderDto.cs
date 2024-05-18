//using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileManagementService.Models
{
    [XmlRoot("PurchaseOrderHeader")]
    public partial class PurchaseOrderHeaderDto
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [XmlElement("PurchaseOrderID")]
        public int PurchaseOrderId { get; set; }
        /// <summary>
        /// Incremental number to track changes to the purchase order over time.
        /// </summary>
        [XmlElement("RevisionNumber")]
        public byte RevisionNumber { get; set; }
        /// <summary>
        /// Order current status. 1 = Pending; 2 = Approved; 3 = Rejected; 4 = Complete
        /// </summary>
        [XmlElement("Status")]
        public byte Status { get; set; }
        /// <summary>
        /// Employee who created the purchase order. Foreign key to Employee.BusinessEntityID.
        /// </summary>
        [XmlElement("EmployeeID")]
        public int EmployeeId { get; set; }
        /// <summary>
        /// Vendor with whom the purchase order is placed. Foreign key to Vendor.BusinessEntityID.
        /// </summary>
        [XmlElement("VendorID")]
        public int VendorId { get; set; }
        /// <summary>
        /// Shipping method. Foreign key to ShipMethod.ShipMethodID.
        /// </summary>
        [XmlElement("ShipMethodID")]
        public int ShipMethodId { get; set; }
        /// <summary>
        /// Purchase order creation date.
        /// </summary>
        [XmlElement("OrderDate")]
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// Estimated shipment date from the vendor.
        /// </summary>
        [XmlElement("ShipDate")]
        public DateTime? ShipDate { get; set; }
        /// <summary>
        /// Purchase order subtotal. Computed as SUM(PurchaseOrderDetail.LineTotal)for the appropriate PurchaseOrderID.
        /// </summary>
        [XmlElement("SubTotal")]
        public decimal SubTotal { get; set; }
        /// <summary>
        /// Tax amount.
        /// </summary>
        [XmlElement("TaxAmt")]
        public decimal TaxAmt { get; set; }
        /// <summary>
        /// Shipping cost.
        /// </summary>
        [XmlElement("Freight")]
        public decimal Freight { get; set; }
        /// <summary>
        /// Total due to vendor. Computed as Subtotal + TaxAmt + Freight.
        /// </summary>
        [XmlElement("TotalDue")]
        public decimal TotalDue { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        [XmlElement("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }

        //public class PurchaseOrderHeaderValidator : AbstractValidator<PurchaseOrderHeaderDto>
        //{
        //    public PurchaseOrderHeaderValidator()
        //    {
        //        RuleFor(x => x.VendorId).NotEmpty().WithMessage("VendorId is required.");
        //        RuleFor(x => x.TotalDue).NotEmpty().WithMessage("TotalDue is required.");
        //        RuleFor(x => x.PurchaseOrderId).NotEmpty().WithMessage("PurchaseOrderId is required.");
        //    }
        //}
    }
}
