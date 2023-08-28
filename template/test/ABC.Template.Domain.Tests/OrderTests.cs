using ABC.Template.Domain.AggregatesModel.OrderAggregate;
namespace ABC.Template.Domain.Tests
{
    public class OrderTests
    {
        [Fact]
        public void OrderPaid_Test()
        {
            Order order = new Order("test", 1);
            Assert.False(order.Paid);
            order.OrderPaid();
            Assert.True(order.Paid);
        }
    }
}