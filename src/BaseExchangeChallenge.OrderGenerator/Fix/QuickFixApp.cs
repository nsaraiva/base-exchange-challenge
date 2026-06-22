using BaseExchangeChallenge.OrderGenerator.Services;
using QuickFix;
using QuickFix.Fields;

namespace BaseExchangeChallenge.OrderGenerator.Fix;

public sealed class QuickFixApp : IApplication
{
    private readonly ExecutionReportAwaiter _awaiter;
    public SessionID? ActiveSessionId { get; private set; }

    public QuickFixApp(ExecutionReportAwaiter awaiter)
    {
        _awaiter = awaiter;
    }

    public void OnCreate(SessionID sessionID) { }

    public void OnLogon(SessionID sessionID)
    {
        ActiveSessionId = sessionID;
        Console.WriteLine($"[FIX] Logon: {sessionID}");
    }

    public void OnLogout(SessionID sessionID)
    {
        if (ActiveSessionId == sessionID) ActiveSessionId = null;
        Console.WriteLine($"[FIX] Logout: {sessionID}");
    }

    public void ToAdmin(Message message, SessionID sessionID) { }
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) { }

    public void FromApp(Message message, SessionID sessionID)
    {
        var msgType = message.Header.GetString(Tags.MsgType);
        if (msgType != MsgType.EXECUTION_REPORT) return;

        var clOrdId = message.IsSetField(Tags.ClOrdID) ? message.GetString(Tags.ClOrdID) : "";
        var execType = message.IsSetField(Tags.ExecType) ? message.GetChar(Tags.ExecType) : '\0';
        var ordStatus = message.IsSetField(Tags.OrdStatus) ? message.GetChar(Tags.OrdStatus) : '\0';
        var text = message.IsSetField(Tags.Text) ? message.GetString(Tags.Text) : "";

        var accepted = execType == ExecType.NEW && ordStatus == OrdStatus.NEW;

        _awaiter.Complete(new ExecutionReportResult
        {
            ClOrdId = clOrdId,
            Accepted = accepted,
            ExecType = execType == '\0' ? "" : execType.ToString(),
            OrdStatus = ordStatus == '\0' ? "" : ordStatus.ToString(),
            Text = text
        });
    }
}