using BaseExchangeChallenge.OrderAccumulator.Fix;
using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using Moq;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Shouldly;

namespace BaseExchangeChallenge.OrderAccumulator.Tests.Unit;

public class QuickFixAppTests
{
    [Fact]
    public void OnMessage_WhenBasicValidationFails_ShouldCreateRejectedExecutionReport()
    {
        var validator = new Mock<IOrderValidator>();
        var exposureBook = new Mock<IExposureBook>();
        var reportFactory = new Mock<IExecutionReportFactory>();

        validator
            .Setup(v => v.Validate(It.IsAny<NewOrderSingle>(), out It.Ref<string>.IsAny))
            .Returns((NewOrderSingle _, out string reason) =>
            {
                reason = "invalid order";
                return false;
            });

        var expectedReport = BuildExecReport(accepted: false);
        reportFactory
            .Setup(f => f.Create(It.IsAny<NewOrderSingle>(), false, It.IsAny<string>()))
            .Returns(expectedReport);

        var sut = new QuickFixApp(validator.Object, exposureBook.Object, reportFactory.Object);
        var order = BuildOrder("PETR4", Side.BUY, 10m, 100m);

        // chama diretamente a regra
        var ex = Record.Exception(() => sut.OnMessage(order, new SessionID("FIX.4.4", "S", "T")));

        // sem sessão FIX real, SendToTarget pode lançar; o foco é orquestração prévia
        ex.ShouldNotBeNull();

        exposureBook.Verify(b => b.TryApply(
            It.IsAny<string>(),
            It.IsAny<char>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            out It.Ref<decimal>.IsAny,
            out It.Ref<string>.IsAny), Times.Never);

        reportFactory.Verify(f => f.Create(It.IsAny<NewOrderSingle>(), false, "invalid order"), Times.Once);
    }

    [Fact]
    public void OnMessage_WhenValidationPasses_ShouldConsultExposureBook()
    {
        var validator = new Mock<IOrderValidator>();
        var exposureBook = new Mock<IExposureBook>();
        var reportFactory = new Mock<IExecutionReportFactory>();

        validator
            .Setup(v => v.Validate(It.IsAny<NewOrderSingle>(), out It.Ref<string>.IsAny))
            .Returns((NewOrderSingle _, out string reason) =>
            {
                reason = "ok";
                return true;
            });

        exposureBook
            .Setup(b => b.TryApply(
                It.IsAny<string>(),
                It.IsAny<char>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                out It.Ref<decimal>.IsAny,
                out It.Ref<string>.IsAny))
            .Returns((string _, char _, decimal _, decimal _, out decimal projected, out string reason) =>
            {
                projected = 10_000m;
                reason = "accepted";
                return true;
            });

        reportFactory
            .Setup(f => f.Create(It.IsAny<NewOrderSingle>(), true, It.IsAny<string>()))
            .Returns(BuildExecReport(true));

        var sut = new QuickFixApp(validator.Object, exposureBook.Object, reportFactory.Object);
        var order = BuildOrder("PETR4", Side.BUY, 10m, 1000m);

        var ex = Record.Exception(() => sut.OnMessage(order, new SessionID("FIX.4.4", "S", "T")));
        ex.ShouldNotBeNull();

        exposureBook.Verify(b => b.TryApply(
            "PETR4",
            Side.BUY,
            10m,
            1000m,
            out It.Ref<decimal>.IsAny,
            out It.Ref<string>.IsAny), Times.Once);

        reportFactory.Verify(f => f.Create(It.IsAny<NewOrderSingle>(), true, It.Is<string>(s => s.Contains("ProjectedExposure"))), Times.Once);
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

    private static ExecutionReport BuildExecReport(bool accepted) =>
        new(
            new OrderID(Guid.NewGuid().ToString("N")),
            new ExecID(Guid.NewGuid().ToString("N")),
            new ExecType(accepted ? ExecType.NEW : ExecType.REJECTED),
            new OrdStatus(accepted ? OrdStatus.NEW : OrdStatus.REJECTED),
            new Symbol("PETR4"),
            new Side(Side.BUY),
            new LeavesQty(0),
            new CumQty(0),
            new AvgPx(0));
}