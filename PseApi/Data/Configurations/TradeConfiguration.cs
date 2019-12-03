using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PseApi.Data.Configurations
{
    public class TradeConfiguration : IEntityTypeConfiguration<Trade>
    {
        public void Configure(EntityTypeBuilder<Trade> builder)
        {
            builder.ToTable("trades");

            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.Date);
            builder.HasIndex(e => e.BIC);
            builder.HasIndex(e => e.ISIN);

            builder.Property(e => e.Id)
                .HasColumnType("bigint");

            builder.Property(e => e.ISIN)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.BIC)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.Change)
               .HasColumnType("decimal(16,4)");

            builder.Property(e => e.Close)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.Open)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.DayMax)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.DayMin)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.YearMin)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.YearMax)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.Previous)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.TradedAmount)
                .HasColumnType("decimal(16,4)");

            builder.Property(e => e.Date)
                .HasColumnType("date");

            builder.Property(e => e.LastTrade)
                .HasColumnType("date");

            builder.Property(e => e.Volume)
                .HasColumnType("bigint");

            builder.Property(e => e.LotSize)
                .HasColumnType("int");

            builder.Property(e => e.Name)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.MarketCode)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.MarketGroup)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.Mode)
                .HasColumnType("varchar(255)");
        }
    }
}
