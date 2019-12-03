using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PseApi.Data.Configurations
{
    public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
    {
        public void Configure(EntityTypeBuilder<Dataset> builder)
        {
            builder.ToTable("days");

            builder.HasKey(e => e.Day);

            builder.Property(e => e.Day)
                .HasColumnType("date");
        }
    }
}
