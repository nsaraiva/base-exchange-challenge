using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace BaseExchangeChallenge.OrderAccumulator.Fix.Services;

public sealed class DefaultOrderValidator : IOrderValidator
{
    public bool Validate(NewOrderSingle order, out string reason)
    {
        var symbol = order.Symbol.Value;
        var side = order.Side.Value;
        var qty = order.OrderQty.Value;
        var price = order.Price.Value;

        var isValidSymbol = symbol is "PETR4" or "VALE3" or "VIIA4";
        if (!isValidSymbol)
        {
            reason = "Invalid symbol";
            return false;
        }

        var isValidSide = side == Side.BUY || side == Side.SELL;
        if (!isValidSide)
        {
            reason = "Invalid side";
            return false;
        }

        var isValidQty = qty >= 1 && qty <= 99_999;
        if (!isValidQty)
        {
            reason = "Invalid quantity range";
            return false;
        }

        var isValidPrice = price >= 0.01m && price <= 999.99m;
        if (!isValidPrice)
        {
            reason = "Invalid price range";
            return false;
        }

        reason = "Order accepted";
        return true;
    }
}