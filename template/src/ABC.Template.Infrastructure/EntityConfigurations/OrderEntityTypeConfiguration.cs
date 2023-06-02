using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.Reflection.Emit;

namespace ABC.Template.Infrastructure.EntityConfigurations
{
    internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("order");
            builder.HasKey(t => t.Id);
            builder.Property(b => b.Title);
            builder.Ignore(b => b.DomainEvents);
        }
    }
}
