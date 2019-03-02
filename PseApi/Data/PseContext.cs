using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PseApi.Data
{
    public class PseContext : DbContext
    {
        public IConfiguration Configuration { get; }

        public virtual DbSet<Dataset> Datasets { get; set; }
        public virtual DbSet<Trade> Trades { get; set; }
        public virtual DbSet<Stock> Stocks { get; set; }

        public PseContext()
        {
        }

        public PseContext(DbContextOptions<PseContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Configuration.GetConnectionString("Default");
                optionsBuilder.UseMySql(connectionString, mysqlOptions => { });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dataset>(entity =>
            {
                entity.ToTable("days");

                entity.HasKey(e => e.Day);

                entity.Property(e => e.Day)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Trade>(entity =>
            {
                entity.ToTable("trades");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.BIC);
                entity.HasIndex(e => e.ISIN);

                entity.Property(e => e.Id)
                    .HasColumnType("bigint");

                entity.Property(e => e.ISIN)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.BIC)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Change)
                   .HasColumnType("decimal(16,4)");

                entity.Property(e => e.Close)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.Open)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.DayMax)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.DayMin)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.YearMin)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.YearMax)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.Previous)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.TradedAmount)
                    .HasColumnType("decimal(16,4)");

                entity.Property(e => e.Date)
                    .HasColumnType("date");

                entity.Property(e => e.LastTrade)
                    .HasColumnType("date");

                entity.Property(e => e.Volume)
                    .HasColumnType("bigint");

                entity.Property(e => e.LotSize)
                    .HasColumnType("int");

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.MarketCode)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.MarketGroup)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Mode)
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ISIN)
                    .IsUnique();

                entity.HasIndex(e => e.BIC)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint");

                entity.Property(e => e.ISIN)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Name)
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.BIC)
                    .HasColumnType("varchar(255)");
            });
        }
    }
}
