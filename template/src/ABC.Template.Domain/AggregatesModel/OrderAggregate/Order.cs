using ABC.Template.Domain.DomainEvents;
using NetCorePal.Extensions.Domain;

namespace ABC.Template.Domain.AggregatesModel.OrderAggregate
{


    /// <summary>
    /// 聚合根
    /// </summary>
    public class Order : Entity<long>, IAggregateRoot
    {
        public Order(string name, int count)
        {
            this.Name = name;
            this.Count = count;
            this.AddDomainEvent(new OrderCreatedDomainEvent(this));
        }



        public bool Paid { get; private set; } = false;

        public string Name { get; private set; }

        public int Count { get; private set; }


        public void OrderPaid()
        {
            this.Paid = true;
        }
    }
}
