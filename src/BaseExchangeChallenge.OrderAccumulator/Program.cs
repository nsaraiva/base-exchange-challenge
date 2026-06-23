using BaseExchangeChallenge.OrderAccumulator.Fix;
using BaseExchangeChallenge.OrderAccumulator.Fix.Interfaces;
using BaseExchangeChallenge.OrderAccumulator.Fix.Services;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

var cfgPath = Path.Combine(AppContext.BaseDirectory, "fix", "orderaccumulator.cfg");
if (!File.Exists(cfgPath))
{
    Console.WriteLine($"Config file not found: {cfgPath}");
    return;
}

IOrderValidator validator = new DefaultOrderValidator();
IExposureBook exposureBook = new InMemoryExposureBook();
IExecutionReportFactory executionReportFactory = new ExecutionReportFactory();

var app = new QuickFixApp(validator, exposureBook, executionReportFactory);

var settings = new SessionSettings(cfgPath);
var storeFactory = new FileStoreFactory(settings);
var logFactory = new FileLogFactory(settings);
var messageFactory = new DefaultMessageFactory();

using var acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory, messageFactory);

acceptor.Start();
Console.WriteLine("OrderAccumulator started. Press ENTER to stop...");
Console.ReadLine();
acceptor.Stop();
Console.WriteLine("OrderAccumulator stopped.");