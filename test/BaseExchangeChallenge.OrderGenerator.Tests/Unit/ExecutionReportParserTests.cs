using QuickFix.Fields;
using QuickFix.FIX44;
using Shouldly;

namespace BaseExchangeChallenge.OrderGenerator.Tests.Unit;

// Substitua por seu parser/adapter real
public class ExecutionReportParserTests
{
    [Fact]
    public void Parse_WhenAccepted_ShouldReturnAcceptedStatus()
    {
        var report = BuildExecReport(ExecType.NEW, OrdStatus.NEW, "Order accepted");
        var result = Parse(report);

        result.Status.ShouldBe("ACEITA");
        result.ExecType.ShouldBe("0");
        result.OrdStatus.ShouldBe("0");
        result.Message.ShouldContain("accepted");
    }

    [Fact]
    public void Parse_WhenRejected_ShouldReturnRejectedStatus()
    {
        var report = BuildExecReport(ExecType.REJECTED, OrdStatus.REJECTED, "Exposure limit breached");
        var result = Parse(report);

        result.Status.ShouldBe("REJEITADA");
        result.ExecType.ShouldBe("8");
        result.OrdStatus.ShouldBe("8");
        result.Message.ShouldContain("breached");
    }

    // Troque por parser de produção
    private static ParsedReport Parse(ExecutionReport report)
    {
        var accepted = report.ExecType.Value == ExecType.NEW;
        return new ParsedReport
        {
            Status = accepted ? "ACEITA" : "REJEITADA",
            ExecType = report.ExecType.Value.ToString(),
            OrdStatus = report.OrdStatus.Value.ToString(),
            Message = report.IsSetText() ? report.Text.Value : ""
        };
    }

    private static ExecutionReport BuildExecReport(char execType, char ordStatus, string text)
    {
        var r = new ExecutionReport(
            new OrderID(Guid.NewGuid().ToString("N")),
            new ExecID(Guid.NewGuid().ToString("N")),
            new ExecType(execType),
            new OrdStatus(ordStatus),
            new Symbol("PETR4"),
            new Side(Side.BUY),
            new LeavesQty(0),
            new CumQty(0),
            new AvgPx(0));

        r.Set(new Text(text));
        return r;
    }

    private sealed class ParsedReport
    {
        public string Status { get; set; } = "";
        public string ExecType { get; set; } = "";
        public string OrdStatus { get; set; } = "";
        public string Message { get; set; } = "";
    }
}