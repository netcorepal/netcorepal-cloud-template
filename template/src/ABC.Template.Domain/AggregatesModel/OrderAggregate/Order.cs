using ABC.Template.Domain.DomainEvents;
using NetCorePal.Extensions.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABC.Template.Domain.AggregatesModel.OrderAggregate
{
    public partial record OrderId : IInt64StronglyTypedId;

    /// <summary>
    /// 聚合根
    /// </summary>
    public partial class Order : Entity<OrderId>, IAggregateRoot
    {
        /// <summary>
        /// 受保护的默认构造函数，用以作为EF Core的反射入口
        /// </summary>
        protected Order() { }

        public Order(string name, int count)
        {
            this.Name = name;
            this.Count = count;
            this.AddDomainEvent(new OrderCreatedDomainEvent(this));
        }

        public bool Paid { get; private set; } = false;

        public string Name { get; private set; } = string.Empty;

        public int Count { get; private set; }

        public partial void ABC();

        public void OrderPaid()
        {
            this.Paid = true;
        }
    }


    public partial class Order
    {
        public partial void ABC()
        { 

        }
    }
}
