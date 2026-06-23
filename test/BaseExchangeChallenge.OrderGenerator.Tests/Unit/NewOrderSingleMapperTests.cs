using QuickFix.Fields;
using QuickFix.FIX44;
using Shouldly;

namespace BaseExchangeChallenge.OrderGenerator.Tests.Unit;

// Ajuste para sua implementação real (ex.: OrderRequestMapper)
public class NewOrderSingleMapperTests
{
    [Fact]
    public void Build_ShouldMapAllFieldsCorrectly()
    {
        var req = new TestOrderRequest
        {
            Symbol = "PETR4",
            Side = Side.BUY,
            Quantity = 1000m,
            Price = 10.00m
        };

        var msg = BuildNewOrderSingle(req);

        msg.Symbol.Value.ShouldBe("PETR4");
        msg.Side.Value.ShouldBe(Side.BUY);
        msg.OrderQty.Value.ShouldBe(1000m);
        msg.Price.Value.ShouldBe(10.00m);
        msg.OrdType.Value.ShouldBe(OrdType.LIMIT);
        msg.ClOrdID.Value.ShouldNotBeNullOrWhiteSpace();
    }

    // Troque por chamada da sua classe de produção
    private static NewOrderSingle BuildNewOrderSingle(TestOrderRequest req)
    {
        var msg = new NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString("N")),
            new Symbol(req.Symbol), 
            new Side(req.Side),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT));

        msg.Set(new Symbol(req.Symbol));
        msg.Set(new OrderQty(req.Quantity));
        msg.Set(new Price(req.Price));
        return msg;
    }

    private sealed class TestOrderRequest
    {
        public string Symbol { get; set; } = "";
        public char Side { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}