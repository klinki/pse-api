using Microsoft.EntityFrameworkCore;

namespace PseApi.Data
{
    public class PseContext : DbContext
    {
        public virtual DbSet<Dataset> Datasets { get; set; }
        public virtual DbSet<Trade> Trades { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }

        public PseContext()
        {
        }

        public PseContext(DbContextOptions<PseContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PseContext).Assembly);
        }
    }
}
