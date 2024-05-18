﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using DataAccessService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccessService.Data
{
    public partial class BgmDbContext : DbContext
    {
        public BgmDbContext()
        {
        }

        public BgmDbContext(DbContextOptions<BgmDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BusinessEntity> BusinessEntities { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> ProductCategories { get; set; }
        public virtual DbSet<ProductModel> ProductModels { get; set; }
        public virtual DbSet<ProductSubcategory> ProductSubcategories { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public virtual DbSet<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        public virtual DbSet<PurchaseOrderSentChannel> PurchaseOrderSentChannels { get; set; }
        public virtual DbSet<PurchaseOrdersProcessedSent> PurchaseOrdersProcessedSents { get; set; }
        public virtual DbSet<UnitMeasure> UnitMeasures { get; set; }
        public virtual DbSet<VPurchaseOrderSummary> VPurchaseOrderSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessEntity>(entity =>
            {
                entity.ToTable("BusinessEntity", "Person");

                entity.HasComment("Source of the ID that connects vendors, customers, and employees with address and contact information.");

                entity.Property(e => e.BusinessEntityId)
                    .HasColumnName("BusinessEntityID")
                    .HasComment("Primary key for all customers, vendors, and employees.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.Rowguid)
                    .HasColumnName("rowguid")
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product", "Production");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.Class)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.Color).HasMaxLength(15);

                entity.Property(e => e.DiscontinuedDate).HasColumnType("datetime");

                entity.Property(e => e.ListPrice).HasColumnType("money");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProductLine)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.ProductModelId).HasColumnName("ProductModelID");

                entity.Property(e => e.ProductNumber)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.ProductSubcategoryId).HasColumnName("ProductSubcategoryID");

                entity.Property(e => e.Rowguid).HasColumnName("rowguid");

                entity.Property(e => e.SellEndDate).HasColumnType("datetime");

                entity.Property(e => e.SellStartDate).HasColumnType("datetime");

                entity.Property(e => e.Size).HasMaxLength(5);

                entity.Property(e => e.SizeUnitMeasureCode)
                    .HasMaxLength(3)
                    .IsFixedLength();

                entity.Property(e => e.StandardCost).HasColumnType("money");

                entity.Property(e => e.Style)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.Property(e => e.Weight).HasColumnType("decimal(8, 2)");

                entity.Property(e => e.WeightUnitMeasureCode)
                    .HasMaxLength(3)
                    .IsFixedLength();
            });

            modelBuilder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategory", "Production");

                entity.HasComment("High-level product categorization.");

                entity.Property(e => e.ProductCategoryId)
                    .HasColumnName("ProductCategoryID")
                    .HasComment("Primary key for ProductCategory records.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Category description.");

                entity.Property(e => e.Rowguid)
                    .HasColumnName("rowguid")
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            });

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.ToTable("ProductModel", "Production");

                entity.HasComment("Product model classification.");

                entity.Property(e => e.ProductModelId)
                    .HasColumnName("ProductModelID")
                    .HasComment("Primary key for ProductModel records.");

                entity.Property(e => e.CatalogDescription)
                    .HasColumnType("xml")
                    .HasComment("Detailed product catalog information in xml format.");

                entity.Property(e => e.Instructions)
                    .HasColumnType("xml")
                    .HasComment("Manufacturing instructions in xml format.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Product model description.");

                entity.Property(e => e.Rowguid)
                    .HasColumnName("rowguid")
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");
            });

            modelBuilder.Entity<ProductSubcategory>(entity =>
            {
                entity.ToTable("ProductSubcategory", "Production");

                entity.HasComment("Product subcategories. See ProductCategory table.");

                entity.Property(e => e.ProductSubcategoryId)
                    .HasColumnName("ProductSubcategoryID")
                    .HasComment("Primary key for ProductSubcategory records.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Subcategory description.");

                entity.Property(e => e.ProductCategoryId)
                    .HasColumnName("ProductCategoryID")
                    .HasComment("Product category identification number. Foreign key to ProductCategory.ProductCategoryID.");

                entity.Property(e => e.Rowguid)
                    .HasColumnName("rowguid")
                    .HasDefaultValueSql("(newid())")
                    .HasComment("ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.ProductSubcategories)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.HasKey(e => new { e.PurchaseOrderId, e.PurchaseOrderDetailId })
                    .HasName("PK_PurchaseOrderDetail_PurchaseOrderID_PurchaseOrderDetailID");

                entity.ToTable("PurchaseOrderDetail", "Purchasing");

                entity.HasComment("Individual products associated with a specific purchase order. See PurchaseOrderHeader.");

                entity.Property(e => e.PurchaseOrderId)
                    .HasColumnName("PurchaseOrderID")
                    .HasComment("Primary key. Foreign key to PurchaseOrderHeader.PurchaseOrderID.");

                entity.Property(e => e.PurchaseOrderDetailId)
                    .HasColumnName("PurchaseOrderDetailID")
                    .HasComment("Primary key. One line number per purchased product.");

                entity.Property(e => e.DueDate)
                    .HasColumnType("datetime")
                    .HasComment("Date the product is expected to be received.");

                entity.Property(e => e.LineTotal)
                    .HasColumnType("money")
                    .HasComputedColumnSql("(isnull([OrderQty]*[UnitPrice],(0.00)))", false)
                    .HasComment("Per product subtotal. Computed as OrderQty * UnitPrice.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.OrderQty).HasComment("Quantity ordered.");

                entity.Property(e => e.ProductId)
                    .HasColumnName("ProductID")
                    .HasComment("Product identification number. Foreign key to Product.ProductID.");

                entity.Property(e => e.ReceivedQty)
                    .HasColumnType("decimal(8, 2)")
                    .HasComment("Quantity actually received from the vendor.");

                entity.Property(e => e.RejectedQty)
                    .HasColumnType("decimal(8, 2)")
                    .HasComment("Quantity rejected during inspection.");

                entity.Property(e => e.StockedQty)
                    .HasColumnType("decimal(9, 2)")
                    .HasComputedColumnSql("(isnull([ReceivedQty]-[RejectedQty],(0.00)))", false)
                    .HasComment("Quantity accepted into inventory. Computed as ReceivedQty - RejectedQty.");

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("money")
                    .HasComment("Vendor's selling price of a single product.");
            });

            modelBuilder.Entity<PurchaseOrderHeader>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderId)
                    .HasName("PK_PurchaseOrderHeader_PurchaseOrderID");

                entity.ToTable("PurchaseOrderHeader", "Purchasing");

                entity.Property(e => e.PurchaseOrderId)
                    .ValueGeneratedNever()
                    .HasColumnName("PurchaseOrderID");

                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");

                entity.Property(e => e.Freight).HasColumnType("money");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.ShipDate).HasColumnType("datetime");

                entity.Property(e => e.ShipMethodId).HasColumnName("ShipMethodID");

                entity.Property(e => e.SubTotal).HasColumnType("money");

                entity.Property(e => e.TaxAmt).HasColumnType("money");

                entity.Property(e => e.TotalDue)
                    .HasColumnType("money")
                    .HasComputedColumnSql("(isnull(([SubTotal]+[TaxAmt])+[Freight],(0)))", true);

                entity.Property(e => e.VendorId).HasColumnName("VendorID");
            });

            modelBuilder.Entity<PurchaseOrderSentChannel>(entity =>
            {
                entity.ToTable("PurchaseOrderSentChannel", "Purchasing");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<PurchaseOrdersProcessedSent>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderDetailId)
                    .HasName("PK__Purchase__5026B6989D9529CA");

                entity.ToTable("PurchaseOrdersProcessedSent", "Purchasing");

                entity.Property(e => e.PurchaseOrderDetailId).ValueGeneratedNever();

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<UnitMeasure>(entity =>
            {
                entity.HasKey(e => e.UnitMeasureCode)
                    .HasName("PK_UnitMeasure_UnitMeasureCode");

                entity.ToTable("UnitMeasure", "Production");

                entity.HasComment("Unit of measure lookup table.");

                entity.Property(e => e.UnitMeasureCode)
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .HasComment("Primary key.");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())")
                    .HasComment("Date and time the record was last updated.");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Unit of measure description.");
            });

            modelBuilder.Entity<VPurchaseOrderSummary>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vPurchaseOrderSummary", "Purchasing");

                entity.Property(e => e.Freight).HasColumnType("money");

                entity.Property(e => e.LineTotal).HasColumnType("money");

                entity.Property(e => e.OrderDate).HasColumnType("datetime");

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProductNumber)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.PurchaseOrderDetailId).HasColumnName("PurchaseOrderDetailID");

                entity.Property(e => e.PurchaseOrderId).HasColumnName("PurchaseOrderID");

                entity.Property(e => e.SubTotal).HasColumnType("money");

                entity.Property(e => e.TaxAmt).HasColumnType("money");

                entity.Property(e => e.TotalDue).HasColumnType("money");

                entity.Property(e => e.UnitPrice).HasColumnType("money");

                entity.Property(e => e.VendorId).HasColumnName("VendorID");

                entity.Property(e => e.VendorName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}