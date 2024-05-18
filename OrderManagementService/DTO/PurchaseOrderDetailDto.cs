//using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OrderManagementService.DTO
{
    [XmlRoot("PurchaseOrderDetail")]
    public partial class PurchaseOrderDetailDto
    {
        /// <summary>
        /// Primary key. Foreign key to PurchaseOrderHeader.PurchaseOrderID.
        /// </summary>
        [XmlAttribute("PurchaseOrderID")]
        public int PurchaseOrderId { get; set; }
        /// <summary>
        /// Primary key. One line number per purchased product.
        /// </summary>
        [XmlAttribute("PurchaseOrderDetailID")]
        public int PurchaseOrderDetailId { get; set; }
        //public int XmlPurchaseOrderDetailId { get; set; }
        /// <summary>
        /// Date the product is expected to be received.
        /// </summary>
        [XmlElement("DueDate")]
        public DateTime? DueDate { get; set; }
        /// <summary>
        /// Quantity ordered.
        /// </summary>
        [XmlElement("OrderQty")]
        public short OrderQty { get; set; }
        /// <summary>
        /// Product identification number. Foreign key to Product.ProductID.
        /// </summary>
        [XmlElement("ProductID")]
        public int ProductId { get; set; }
        /// <summary>
        /// Vendor&apos;s selling price of a single product.
        /// </summary>
        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// Per product subtotal. Computed as OrderQty * UnitPrice.
        /// </summary>
        [XmlElement("LineTotal")]
        public decimal LineTotal { get; set; }
        /// <summary>
        /// Quantity actually received from the vendor.
        /// </summary>
        [XmlElement("ReceivedQty")]
        public decimal ReceivedQty { get; set; }
        /// <summary>
        /// Quantity rejected during inspection.
        /// </summary>
        [XmlElement("RejectedQty")]
        public decimal RejectedQty { get; set; }
        /// <summary>
        /// Quantity accepted into inventory. Computed as ReceivedQty - RejectedQty.
        /// </summary>
        [XmlElement("StockedQty")]
        public decimal StockedQty { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        [XmlElement("ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }

    //public class PurchaseOrderDetailValidator : AbstractValidator<PurchaseOrderDetailDto>
    //{
    //    public PurchaseOrderDetailValidator()
    //    {
    //        RuleFor(x => x.DueDate).NotEmpty().WithMessage("Due date is required.");
    //        RuleFor(x => x.PurchaseOrderDetailId).NotEmpty().WithMessage("PurchaseOrderDetailId is required.");
    //        RuleFor(x => x.PurchaseOrderId).NotEmpty().WithMessage("PurchaseOrderId is required.");
    //    }
    //}
}
