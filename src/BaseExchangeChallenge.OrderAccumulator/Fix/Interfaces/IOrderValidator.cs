using QuickFix.FIX44;

namespace BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;

public interface IOrderValidator
{
    bool Validate(NewOrderSingle order, out string reason);
}