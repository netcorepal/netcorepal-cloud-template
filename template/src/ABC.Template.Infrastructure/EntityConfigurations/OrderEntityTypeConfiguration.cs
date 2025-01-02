using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABC.Template.Infrastructure.EntityConfigurations
{
    internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("order");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseSnowFlakeValueGenerator();
            builder.Property(b => b.Name).HasMaxLength(100);
            builder.Property(b => b.Count);
            builder.Property(b => b.Paid);
        }
    }

}
