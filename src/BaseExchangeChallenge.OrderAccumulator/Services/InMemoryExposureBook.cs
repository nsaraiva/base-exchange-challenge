using System.Collections.Concurrent;
using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using QuickFix.Fields;

namespace BaseExchangeChallenge.OrderAccumulator.Fix.Services;

public sealed class InMemoryExposureBook : IExposureBook
{
    private const decimal ExposureLimit = 100_000_000m; // R$ 100 milhões

    private readonly ConcurrentDictionary<string, decimal> _exposureBySymbol = new();
    private readonly object _sync = new();

    public bool TryApply(
        string symbol,
        char side,
        decimal price,
        decimal qty,
        out decimal projectedExposure,
        out string reason)
    {
        lock (_sync)
        {
            var current = _exposureBySymbol.TryGetValue(symbol, out var value) ? value : 0m;

            var notional = price * qty;
            var delta = side == Side.BUY ? notional : -notional;

            projectedExposure = current + delta;

            if (Math.Abs(projectedExposure) > ExposureLimit)
            {
                reason = $"Exposure limit breached for {symbol}. Current={current:0.00}, Projected={projectedExposure:0.00}, Limit={ExposureLimit:0.00}";
                return false;
            }

            _exposureBySymbol[symbol] = projectedExposure;
            reason = $"Order accepted. New exposure for {symbol} = {projectedExposure:0.00}";
            return true;
        }
    }

    public decimal GetExposure(string symbol)
    {
        return _exposureBySymbol.TryGetValue(symbol, out var value) ? value : 0m;
    }
}