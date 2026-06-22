using QuickFix.FIX44;

namespace BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;

public interface IExecutionReportFactory
{
    ExecutionReport Create(NewOrderSingle order, bool accepted, string reason);
}