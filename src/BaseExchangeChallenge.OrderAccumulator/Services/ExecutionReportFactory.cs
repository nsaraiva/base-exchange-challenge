using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace BaseExchangeChallenge.OrderAccumulator.Fix.Services;

public sealed class ExecutionReportFactory : IExecutionReportFactory
{
    public ExecutionReport Create(NewOrderSingle order, bool accepted, string reason)
    {
        var side = order.Side.Value;
        var qty = order.OrderQty.Value;
        var clOrdId = order.ClOrdID.Value;
        var symbol = order.Symbol.Value;

        var execReport = new ExecutionReport(
            new OrderID(Guid.NewGuid().ToString("N")),
            new ExecID(Guid.NewGuid().ToString("N")),
            new ExecType(accepted ? ExecType.NEW : ExecType.REJECTED),
            new OrdStatus(accepted ? OrdStatus.NEW : OrdStatus.REJECTED),
            new Symbol(symbol),                 // <- obrigatório na sua assinatura
            new Side(side),
            new LeavesQty(accepted ? qty : 0),
            new CumQty(0),
            new AvgPx(0)
        );

        execReport.SetField(new ClOrdID(clOrdId));
        execReport.SetField(new OrderQty(qty));
        execReport.SetField(new Text(reason));

        return execReport;
    }
}