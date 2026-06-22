using QuickFix;
using QfMessage = QuickFix.Message;

namespace BaseExchangeChallenge.OrderAccumulator.Fix;

public abstract class QuickFixApplicationBase : IApplication
{
    public virtual void OnCreate(SessionID sessionID) { }
    public virtual void OnLogon(SessionID sessionID) { }
    public virtual void OnLogout(SessionID sessionID) { }

    // no-op by default
    public virtual void ToAdmin(QfMessage message, SessionID sessionID) { }
    public virtual void FromAdmin(QfMessage message, SessionID sessionID) { }
    public virtual void ToApp(QfMessage message, SessionID sessionID) { }
    public virtual void FromApp(QfMessage message, SessionID sessionID) { }
}