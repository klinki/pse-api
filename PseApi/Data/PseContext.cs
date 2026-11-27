using Microsoft.EntityFrameworkCore;

namespace PseApi.Data
{
    public class PseContext : DbContext
    {
        /// <summary>
        /// Case-insensitive ICU collation used for stock identifiers (BIC, ISIN), so lookups behave
        /// like they did on MySQL (utf8mb3_general_ci), e.g. <c>/api/trades/isin/cz0005112300</c>.
        /// </summary>
        public const string CaseInsensitiveCollation = "case_insensitive";

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
            modelBuilder.HasCollation(CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PseContext).Assembly);
        }
    }
}
