using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PseApi.Data.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasIndex(e => e.ISIN)
                .IsUnique();

            builder.HasIndex(e => e.BIC)
                .IsUnique();

            builder.Property(e => e.Id)
                .HasColumnType("bigint");

            builder.Property(e => e.ISIN)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.Name)
                .HasColumnType("varchar(255)");

            builder.Property(e => e.BIC)
                .HasColumnType("varchar(255)");
        }
    }
}
