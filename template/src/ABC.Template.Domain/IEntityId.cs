using NetCorePal.Extensions.Domain;
using NetCorePal.Extensions.Domain.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ABC.Template.Domain
{
    [TypeConverter(typeof(EntityIdTypeConverter))]
    public record OrderId(long Id) : IEntityId
    {
        public static implicit operator long(OrderId id) => id.Id;
        public static implicit operator OrderId(long id) => new OrderId(id);


        public override string ToString()
        {
            return Id.ToString();
        }
    }

}
