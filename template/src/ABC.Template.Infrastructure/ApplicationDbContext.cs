using NetCorePal.Extensions.Repository.EntityframeworkCore;
using ABC.Template.Domain.AggregatesModel.OrderAggregate;
using ABC.Template.Infrastructure.EntityConfigurations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ABC.Template.Domain;

namespace ABC.Template.Infrastructure
{
    public class ApplicationDbContext : EFContext
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
        }


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<OrderId>().HaveConversion<OrderIdValueConverter>();
            base.ConfigureConventions(configurationBuilder);
        }

        public DbSet<Order> Orders => Set<Order>();

    }
}