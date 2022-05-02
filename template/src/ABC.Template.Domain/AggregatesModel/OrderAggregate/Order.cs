using ABC.Extensions.Domain;

namespace ABC.Template.Domain.AggregatesModel.OrderAggregate
{
    /// <summary>
    /// 聚合根
    /// </summary>
    public class Order : Entity<long>, IAggregateRoot
    {
        public string Title { get; private set; } = string.Empty;
    }
}
