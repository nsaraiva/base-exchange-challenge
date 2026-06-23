namespace BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;

public interface IExposureBook
{
    bool TryApply(
        string symbol,
        char side,
        decimal price,
        decimal qty,
        out decimal projectedExposure,
        out string reason);

    decimal GetExposure(string symbol);
}