namespace DataAccessService.Dto
{
    public partial class PurchaseOrderHeaderDto
    {
        public int PurchaseOrderId { get; set; }
        public byte RevisionNumber { get; set; }
        public byte Status { get; set; }
        public int EmployeeId { get; set; }
        public int VendorId { get; set; }
        public int ShipMethodId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
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
