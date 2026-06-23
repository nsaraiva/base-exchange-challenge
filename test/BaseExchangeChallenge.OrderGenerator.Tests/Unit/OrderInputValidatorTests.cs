using Shouldly;

namespace BaseExchangeChallenge.OrderGenerator.Tests.Unit;

// Substitua por seu validator real (ex.: DefaultOrderInputValidator)
public class OrderInputValidatorTests
{
    [Theory]
    [InlineData("", '1', 100, 10.0, false)]
    [InlineData("PETR4", '1', 0, 10.0, false)]
    [InlineData("PETR4", '1', 100, 0.0, false)]
    [InlineData("PETR4", '1', 100, 10.0, true)]
    [InlineData("VALE3", '2', 99999, 999.99, true)]
    public void Validate_ShouldReturnExpected(string symbol, char side, decimal qty, decimal price, bool expected)
    {
        var ok = Validate(symbol, side, qty, price, out var reason);

        ok.ShouldBe(expected);
        reason.ShouldNotBeNull(); // motivo sempre preenchido
    }

    // Troque por sua implementação real
    private static bool Validate(string symbol, char side, decimal qty, decimal price, out string reason)
    {
        if (string.IsNullOrWhiteSpace(symbol)) { reason = "Symbol is required"; return false; }
        if (side != '1' && side != '2') { reason = "Side must be BUY(1) or SELL(2)"; return false; }
        if (qty <= 0 || qty > 99999) { reason = "Invalid quantity"; return false; }
        if (price <= 0 || price > 999.99m) { reason = "Invalid price"; return false; }

        reason = "OK";
        return true;
    }
}