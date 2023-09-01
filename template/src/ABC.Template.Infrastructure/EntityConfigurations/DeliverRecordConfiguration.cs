using ABC.Template.Domain;
using ABC.Template.Domain.AggregatesModel.DeliverAggregate;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NetCorePal.Extensions.Snowflake;
using System.Reflection.Emit;

namespace ABC.Template.Infrastructure.EntityConfigurations
{
    internal class DeliverRecordConfiguration : IEntityTypeConfiguration<DeliverRecord>
    {
        public void Configure(EntityTypeBuilder<DeliverRecord> builder)
        {
            builder.ToTable("deliverrecord");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).ValueGeneratedOnAdd().UseSnowFlakeValueGenerator();
        }
    }

}
