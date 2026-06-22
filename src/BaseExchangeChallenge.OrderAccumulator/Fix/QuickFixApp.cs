using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using QuickFix;
using QuickFix.FIX44;
using QfMessage = QuickFix.Message;

namespace BaseExchangeChallenge.OrderAccumulator.Fix;

public sealed class QuickFixApp : MessageCracker, IApplication
{
    private readonly IOrderValidator _validator;
    private readonly IExposureBook _exposureBook;
    private readonly IExecutionReportFactory _executionReportFactory;

    public QuickFixApp(
        IOrderValidator validator,
        IExposureBook exposureBook,
        IExecutionReportFactory executionReportFactory)
    {
        _validator = validator;
        _exposureBook = exposureBook;
        _executionReportFactory = executionReportFactory;
    }

    public void OnCreate(SessionID sessionID) =>
        Console.WriteLine($"[ACCEPTOR] OnCreate: {sessionID}");

    public void OnLogon(SessionID sessionID) =>
        Console.WriteLine($"[ACCEPTOR] OnLogon: {sessionID}");

    public void OnLogout(SessionID sessionID) =>
        Console.WriteLine($"[ACCEPTOR] OnLogout: {sessionID}");

    public void ToAdmin(QfMessage message, SessionID sessionID) { }
    public void FromAdmin(QfMessage message, SessionID sessionID) { }
    public void ToApp(QfMessage message, SessionID sessionID) { }

    public void FromApp(QfMessage message, SessionID sessionID)
    {
        base.Crack(message, sessionID);
    }

    public void OnMessage(NewOrderSingle order, SessionID sessionID)
    {
        var basicValid = _validator.Validate(order, out var reason);

        var accepted = basicValid;
        if (basicValid)
        {
            var symbol = order.Symbol.Value;
            var side = order.Side.Value;
            var qty = order.OrderQty.Value;
            var price = order.Price.Value;

            accepted = _exposureBook.TryApply(
                symbol,
                side,
                price,
                qty,
                out var projectedExposure,
                out var exposureReason);

            reason = accepted
                ? $"{exposureReason} (ProjectedExposure={projectedExposure:0.00})"
                : exposureReason;
        }

        var execReport = _executionReportFactory.Create(order, accepted, reason);
        Session.SendToTarget(execReport, sessionID);
    }
}