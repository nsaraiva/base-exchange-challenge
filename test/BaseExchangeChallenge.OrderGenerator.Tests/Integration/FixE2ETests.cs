using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using Shouldly;
using System.Collections.Concurrent;
using QfMessage = QuickFix.Message;

namespace BaseExchangeChallenge.OrderGenerator.Tests.Integration;

public class FixE2ETests
{
    [Fact]
    public async Task E2E_ShouldReceiveRejected_WhenCumulativeExposureBreachesLimit()
    {
        // Ajuste caminhos conforme sua solução
        var accCfg = Path.Combine(AppContext.BaseDirectory, "fixtures", "orderaccumulator-test.cfg");
        var genCfg = Path.Combine(AppContext.BaseDirectory, "fixtures", "ordergenerator-test.cfg");

        File.Exists(accCfg).ShouldBeTrue($"cfg not found: {accCfg}");
        File.Exists(genCfg).ShouldBeTrue($"cfg not found: {genCfg}");

        var accApp = new AccumulatorStubApp(); // Substitua por QuickFixApp real do accumulator se quiser integração total
        var genApp = new GeneratorProbeApp();

        var accSettings = new SessionSettings(accCfg);
        var genSettings = new SessionSettings(genCfg);

        using var acceptor = new ThreadedSocketAcceptor(
            accApp,
            new MemoryStoreFactory(),
            accSettings,
            new ScreenLogFactory(accSettings),
            new DefaultMessageFactory());

        using var initiator = new SocketInitiator(
            genApp,
            new MemoryStoreFactory(),
            genSettings,
            new ScreenLogFactory(genSettings),
            new DefaultMessageFactory());

        acceptor.Start();
        initiator.Start();

        try
        {
            await genApp.WaitLogon(TimeSpan.FromSeconds(10));

            // Ordem 1: aceita (quase limite)
            var ord1 = BuildOrder("PETR4", Side.BUY, 99999m, 999.99m);
            Session.SendToTarget(ord1, genApp.SessionId!);

            // Ordem 2: deve rejeitar por ultrapassar 100MM cumulativo
            var ord2 = BuildOrder("PETR4", Side.BUY, 3m, 999.99m);
            Session.SendToTarget(ord2, genApp.SessionId!);

            var reports = await genApp.WaitReports(count: 2, timeout: TimeSpan.FromSeconds(10));

            reports.Count.ShouldBe(2);
            reports[0].ExecType.Value.ShouldBe(ExecType.NEW);
            reports[1].ExecType.Value.ShouldBe(ExecType.REJECTED);
            reports[1].Text.Value.ShouldContain("Exposure");
        }
        finally
        {
            initiator.Stop();
            acceptor.Stop();
        }
    }

    private static NewOrderSingle BuildOrder(string symbol, char side, decimal qty, decimal price)
    {
        var msg = new NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString("N")),
            new Symbol(symbol),
            new Side(side),
            new TransactTime(DateTime.UtcNow),
            new OrdType(OrdType.LIMIT));

        msg.Set(new Symbol(symbol));
        msg.Set(new OrderQty(qty));
        msg.Set(new Price(price));
        return msg;
    }

    // Probe do Generator (captura ExecutionReport)
    private sealed class GeneratorProbeApp : MessageCracker, IApplication
    {
        private readonly TaskCompletionSource<bool> _logonTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentQueue<ExecutionReport> _reports = new();

        public SessionID? SessionId { get; private set; }

        public Task WaitLogon(TimeSpan timeout) => WaitWithTimeout(_logonTcs.Task, timeout);

        public async Task<IReadOnlyList<ExecutionReport>> WaitReports(int count, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (_reports.Count < count && DateTime.UtcNow - start < timeout)
                await Task.Delay(50);

            return _reports.ToArray();
        }

        public void OnCreate(SessionID sessionID) => SessionId = sessionID;
        public void OnLogon(SessionID sessionID) => _logonTcs.TrySetResult(true);
        public void OnLogout(SessionID sessionID) { }
        public void ToAdmin(QfMessage message, SessionID sessionID) { }
        public void FromAdmin(QfMessage message, SessionID sessionID) { }
        public void ToApp(QfMessage message, SessionID sessionID) { }
        public void FromApp(QfMessage message, SessionID sessionID) => Crack(message, sessionID);
        public void OnMessage(ExecutionReport msg, SessionID sessionID) => _reports.Enqueue(msg);

        private static async Task WaitWithTimeout(Task task, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            var completed = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token));
            if (completed != task) throw new TimeoutException("Logon timeout.");
            await task;
        }
    }

    // Stub mínimo do accumulator para e2e do gerador (troque pelo app real para integração full stack)
    private sealed class AccumulatorStubApp : MessageCracker, IApplication
    {
        private readonly Dictionary<string, decimal> _exp = new();
        private const decimal Limit = 100_000_000m;

        public void OnCreate(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void ToAdmin(QfMessage message, SessionID sessionID) { }
        public void FromAdmin(QfMessage message, SessionID sessionID) { }
        public void ToApp(QfMessage message, SessionID sessionID) { }
        public void FromApp(QfMessage message, SessionID sessionID) => Crack(message, sessionID);

        public void OnMessage(NewOrderSingle order, SessionID sessionID)
        {
            var symbol = order.Symbol.Value;
            var side = order.Side.Value;
            var qty = order.OrderQty.Value;
            var price = order.Price.Value;

            _exp.TryGetValue(symbol, out var current);
            var delta = (side == Side.BUY ? 1 : -1) * (qty * price);
            var projected = current + delta;
            var accepted = Math.Abs(projected) <= Limit;

            if (accepted) _exp[symbol] = projected;

            var er = new ExecutionReport(
                new OrderID(Guid.NewGuid().ToString("N")),
                new ExecID(Guid.NewGuid().ToString("N")),
                new ExecType(accepted ? ExecType.NEW : ExecType.REJECTED),
                new OrdStatus(accepted ? OrdStatus.NEW : OrdStatus.REJECTED),
                new Symbol(symbol),
                new Side(side),
                new LeavesQty(0),
                new CumQty(0),
                new AvgPx(0));

            er.Set(new ClOrdID(order.ClOrdID.Value));
            er.Set(new Text(accepted ? "Accepted" : "Exposure limit breached"));

            Session.SendToTarget(er, sessionID);
        }
    }
}