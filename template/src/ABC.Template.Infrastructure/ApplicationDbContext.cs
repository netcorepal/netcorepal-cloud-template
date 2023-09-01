using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure.EntityConfigurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ABC.Template.Domain;
using ABC.Template.Domain.AggregatesModel.DeliverAggregate;

namespace ABC.Template.Infrastructure
{
    public partial class ApplicationDbContext : EFContext
    {
        public ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options, mediator, provider)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }
            modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeliverRecordConfiguration());
        }


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ConfigureStronglyTypedIdValueConverter(configurationBuilder);
            base.ConfigureConventions(configurationBuilder);
        }

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<DeliverRecord> DeliverRecords => Set<DeliverRecord>();

    }
}