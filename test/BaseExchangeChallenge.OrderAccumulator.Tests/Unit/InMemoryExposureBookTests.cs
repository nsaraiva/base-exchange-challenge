using BaseExchangeChallenge.OrderAccumulator.Fix.Services;
using QuickFix.Fields;
using Shouldly;

namespace BaseExchangeChallenge.OrderAccumulator.Tests.Unit;

public class InMemoryExposureBookTests
{
    [Fact]
    public void TryApply_BuyWithinLimit_ShouldAccept_AndUpdateExposure()
    {
        var sut = new InMemoryExposureBook();

        var accepted = sut.TryApply(
            symbol: "PETR4",
            side: Side.BUY,
            price: 10m,
            qty: 1000m,
            projectedExposure: out var projected,
            reason: out var reason);

        accepted.ShouldBeTrue();
        projected.ShouldBe(10_000m);
        sut.GetExposure("PETR4").ShouldBe(10_000m);
        reason.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TryApply_Sell_ShouldDecreaseExposure()
    {
        var sut = new InMemoryExposureBook();

        sut.TryApply("VALE3", Side.BUY, 100m, 1000m, out _, out _); // +100_000

        var accepted = sut.TryApply(
            "VALE3",
            Side.SELL,
            100m,
            200m,
            out var projected,
            out _); // -20_000

        accepted.ShouldBeTrue();
        projected.ShouldBe(80_000m);
        sut.GetExposure("VALE3").ShouldBe(80_000m);
    }

    [Fact]
    public void TryApply_WhenProjectedExposureBreachesLimit_ShouldReject_AndKeepPreviousExposure()
    {
        var sut = new InMemoryExposureBook();

        sut.TryApply("PETR4", Side.BUY, 999.99m, 99_999m, out var firstProjected, out _)
            .ShouldBeTrue();
        firstProjected.ShouldBe(99_998_000.01m);

        var accepted = sut.TryApply(
            "PETR4",
            Side.BUY,
            999.99m,
            3m,
            out var projected,
            out var reason);

        accepted.ShouldBeFalse();
        projected.ShouldBe(100_000_999.98m);
        sut.GetExposure("PETR4").ShouldBe(99_998_000.01m); // não atualiza em rejeição
        reason.ShouldContain("Exposure limit breached");
    }
}