using NetCorePal.Extensions.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Template.Domain.AggregatesModel.DeliverAggregate
{

    [TypeConverter(typeof(EntityIdTypeConverter))]
    public record DeliverRecordId(long Id) : IEntityId
    {
        public static implicit operator long(DeliverRecordId id) => id.Id;
        public static implicit operator DeliverRecordId(long id) => new DeliverRecordId(id);


        public override string ToString()
        {
            return Id.ToString();
        }
    }

    public class DeliverRecord : Entity<DeliverRecordId>, IAggregateRoot
    {
        protected DeliverRecord() { }


        public DeliverRecord(OrderId orderId)
        {
            this.OrderId = orderId;
        }

        public OrderId OrderId { get; private set; } = default!;
    }
}
