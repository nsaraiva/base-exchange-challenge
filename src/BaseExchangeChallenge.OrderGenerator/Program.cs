using BaseExchangeChallenge.OrderGenerator.Fix;
using BaseExchangeChallenge.OrderGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// FIX dependencies
builder.Services.AddSingleton<ExecutionReportAwaiter>();
builder.Services.AddSingleton<QuickFixApp>();
builder.Services.AddSingleton<FixClientService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Orders}/{action=Index}/{id?}");

// Start FIX initiator on app startup
var fixClient = app.Services.GetRequiredService<FixClientService>();
var env = app.Services.GetRequiredService<IWebHostEnvironment>();
var cfgPath = Path.Combine(env.ContentRootPath, "fix", "ordergenerator.cfg");
fixClient.Start(cfgPath);

// Stop FIX initiator gracefully
app.Lifetime.ApplicationStopping.Register(() =>
{
    fixClient.Stop();
});

await app.RunAsync();