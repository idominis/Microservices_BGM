using DataAccessService.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessService.Data
{
    public class BgmDbContext_old : DbContext
    {
        public BgmDbContext_old(DbContextOptions<BgmDbContext_old> options)
            : base(options)
        {
        }

        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PurchaseOrderDetail>().ToTable("PurchaseOrderDetail");
        }
    }
}
