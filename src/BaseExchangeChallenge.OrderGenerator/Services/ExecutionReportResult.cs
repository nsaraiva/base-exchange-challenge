using System.Collections.Concurrent;

namespace BaseExchangeChallenge.OrderGenerator.Services;

public sealed class ExecutionReportResult
{
    public string ClOrdId { get; init; } = "";
    public bool Accepted { get; init; }
    public string ExecType { get; init; } = "";
    public string OrdStatus { get; init; } = "";
    public string Text { get; init; } = "";
}

public sealed class ExecutionReportAwaiter
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ExecutionReportResult>> _pending = new();

    public Task<ExecutionReportResult> WaitAsync(string clOrdId, TimeSpan timeout, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<ExecutionReportResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pending.TryAdd(clOrdId, tcs))
            throw new InvalidOperationException($"ClOrdID já pendente: {clOrdId}");

        var timeoutCts = new CancellationTokenSource(timeout);
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        CancellationTokenRegistration reg = default;
        reg = linkedCts.Token.Register(() =>
        {
            if (_pending.TryRemove(clOrdId, out var pending))
            {
                if (ct.IsCancellationRequested)
                    pending.TrySetCanceled(ct);
                else
                    pending.TrySetException(new TimeoutException($"Timeout aguardando ExecutionReport: {clOrdId}"));
            }

            reg.Dispose();
            linkedCts.Dispose();
            timeoutCts.Dispose();
        });

        // Cleanup quando concluir normalmente (evita ficar inscrição viva)
        _ = tcs.Task.ContinueWith(_ =>
        {
            reg.Dispose();
            linkedCts.Dispose();
            timeoutCts.Dispose();
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    public void Complete(ExecutionReportResult result)
    {
        if (_pending.TryRemove(result.ClOrdId, out var tcs))
            tcs.TrySetResult(result);
    }
}