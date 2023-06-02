using ABC.Template.Domain.DomainEvents;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Domain.AggregatesModel.OrderAggregate
{


    /// <summary>
    /// 聚合根
    /// </summary>
    public class Order : Entity<long>, IAggregateRoot
    {
        public Order(string title)
        {
            this.Title = title;
            this.AddDomainEvent(new OrderCreatedDomainEvent(this));
        }


        public string Title { get; private set; }
    }
}
