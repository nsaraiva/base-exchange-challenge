using BaseExchangeChallenge.OrderGenerator.Fix;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;

namespace BaseExchangeChallenge.OrderGenerator.Services;

public sealed class FixClientService : IDisposable
{
    private readonly QuickFixApp _app;
    private readonly ExecutionReportAwaiter _awaiter;
    private SocketInitiator? _initiator;

    public FixClientService(QuickFixApp app, ExecutionReportAwaiter awaiter)
    {
        _app = app;
        _awaiter = awaiter;
    }

    public void Start(string cfgPath)
    {
        var settings = new SessionSettings(cfgPath);
        var storeFactory = new FileStoreFactory(settings);
        var logFactory = new FileLogFactory(settings);
        var messageFactory = new DefaultMessageFactory();

        _initiator = new SocketInitiator(_app, storeFactory, settings, logFactory, messageFactory);
        _initiator.Start();
    }

    public async Task<ExecutionReportResult> SendOrderAsync(string symbol, string side, int quantity, decimal price, CancellationToken ct)
    {
        var sessionId = _app.ActiveSessionId ?? throw new InvalidOperationException("Sessão FIX não conectada.");

        var clOrdId = Guid.NewGuid().ToString("N");

        var fixSide = side.ToUpperInvariant() switch
        {
            "BUY" => Side.BUY,
            "SELL" => Side.SELL,
            _ => throw new ArgumentException("Side inválido. Use BUY ou SELL.")
        };

        var nos = new NewOrderSingle(
            new ClOrdID(clOrdId),
            new Symbol(symbol),
            new Side(fixSide),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT));

        nos.Set(new OrderQty((decimal)quantity));
        nos.Set(new Price(price));
        nos.Set(new HandlInst(HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE_NO_BROKER_INTERVENTION));

        var sent = Session.SendToTarget(nos, sessionId);
        if (!sent)
            throw new InvalidOperationException("Falha ao enviar mensagem FIX para a sessão.");

        return await _awaiter.WaitAsync(clOrdId, TimeSpan.FromSeconds(10), ct);
    }

    public void Stop() => _initiator?.Stop();

    public void Dispose() => Stop();
}