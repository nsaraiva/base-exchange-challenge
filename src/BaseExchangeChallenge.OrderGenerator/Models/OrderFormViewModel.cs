using System.ComponentModel.DataAnnotations;

namespace BaseExchangeChallenge.OrderGenerator.Models;

public sealed class OrderFormViewModel
{
    [Required]
    [RegularExpression("^(PETR4|VALE3|VIIA4)$", ErrorMessage = "Símbolo inválido.")]
    public string Symbol { get; set; } = "PETR4";

    [Required]
    [RegularExpression("^(BUY|SELL)$", ErrorMessage = "Lado inválido.")]
    public string Side { get; set; } = "BUY";

    [Range(1, 99999, ErrorMessage = "Quantidade deve ser entre 1 e 99.999.")]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0,01", "999,99", ErrorMessage = "Preço deve ser entre 0,01 e 999,99.")]
    public decimal Price { get; set; }
}