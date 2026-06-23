using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using BaseExchangeChallenge.OrderAccumulator.Fix.Services;
using QuickFix.Fields;
using QuickFix.FIX44;
using Shouldly;

namespace BaseExchangeChallenge.OrderAccumulator.Tests.Integration;

public class OrderFlowIntegrationTests
{
    [Fact]
    public void ExposureFlow_CumulativeOrders_ShouldAcceptThenReject_WhenBreachingLimit()
    {
        IExposureBook book = new InMemoryExposureBook();

        // 1) aceita quase no limite
        var ok1 = book.TryApply("PETR4", Side.BUY, 999.99m, 99_999m, out var projected1, out _);
        ok1.ShouldBeTrue();
        projected1.ShouldBe(99_998_000.01m);

        // 2) rejeita ao ultrapassar
        var ok2 = book.TryApply("PETR4", Side.BUY, 999.99m, 3m, out var projected2, out var reason2);
        ok2.ShouldBeFalse();
        projected2.ShouldBe(100_000_999.98m);
        reason2.ShouldContain("Exposure limit breached");

        // estado permanece no valor aceito anterior
        book.GetExposure("PETR4").ShouldBe(99_998_000.01m);
    }

    [Fact]
    public void ExposureFlow_SellShouldReduceExposure()
    {
        IExposureBook book = new InMemoryExposureBook();

        book.TryApply("VALE3", Side.BUY, 1000m, 50_000m, out _, out _).ShouldBeTrue();   // +50M
        book.TryApply("VALE3", Side.SELL, 1000m, 10_000m, out var projected, out _).ShouldBeTrue(); // -10M

        projected.ShouldBe(40_000_000m);
        book.GetExposure("VALE3").ShouldBe(40_000_000m);
    }

    [Fact]
    public void ExecutionReportFactory_ShouldCreateRejectedReport_WhenNotAccepted()
    {
        IExecutionReportFactory factory = new ExecutionReportFactory();
        var order = BuildOrder("PETR4", Side.BUY, 10m, 100m);

        var report = factory.Create(order, accepted: false, reason: "limit breached");

        report.ExecType.Value.ShouldBe(ExecType.REJECTED);
        report.OrdStatus.Value.ShouldBe(OrdStatus.REJECTED);
        report.Text.Value.ShouldContain("limit breached");
    }

    private static NewOrderSingle BuildOrder(string symbol, char side, decimal price, decimal qty)
    {
        var order = new NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString("N")),
            new Symbol(symbol),
            new Side(side),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT));

        order.Set(new Symbol(symbol));
        order.Set(new Price(price));
        order.Set(new OrderQty(qty));
        return order;
    }
}