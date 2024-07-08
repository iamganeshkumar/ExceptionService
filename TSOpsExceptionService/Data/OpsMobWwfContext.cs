using Microsoft.EntityFrameworkCore;
using TSOpsExceptionService.Models;

namespace TSOpsExceptionService.Data
{
    public partial class OpsMobWwfContext : DbContext
    {
        public OpsMobWwfContext(DbContextOptions<OpsMobWwfContext> options)
            : base(options)
        {
        }

        public virtual DbSet<LastWorkFlowException> LastWorkFlowExceptions { get; set; }
        public virtual DbSet<ReprocessedException> ReprocessedExceptions { get; set; }
        public virtual DbSet<WorkflowException> WorkflowExceptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LastWorkFlowException>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<ReprocessedException>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<WorkflowException>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Requests");
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsBusinessError).HasDefaultValue(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
