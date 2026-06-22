using BaseExchangeChallenge.OrderGenerator.Models;
using BaseExchangeChallenge.OrderGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaseExchangeChallenge.OrderGenerator.Controllers;

public sealed class OrdersController : Controller
{
    private readonly FixClientService _fixClient;

    public OrdersController(FixClientService fixClient)
    {
        _fixClient = fixClient;
    }

    [HttpGet]
    public IActionResult Index() => View(new OrderFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(OrderFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        // Regra de múltiplo de 0,01 (extra)
        if (vm.Price * 100m != decimal.Truncate(vm.Price * 100m))
        {
            ModelState.AddModelError(nameof(vm.Price), "Preço deve ser múltiplo de 0,01.");
            return View(vm);
        }

        try
        {
            var result = await _fixClient.SendOrderAsync(vm.Symbol, vm.Side, vm.Quantity, vm.Price, ct);
            ViewBag.Result = result;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(vm);
    }
}