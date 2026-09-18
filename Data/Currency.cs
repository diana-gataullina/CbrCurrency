using System.ComponentModel.DataAnnotations;

namespace CbrCurrency.Data;

public class Currency
{
    public int Id { get; set; }
    
    [Required, MaxLength(10)]
    public string CbrId { get; set; } = null!;
    
    [Required, MaxLength(3)]
    public string CharCode { get; set; } = null!;
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    
    public int Nominal { get; set; }

    public ICollection<ExchangeRate> ExchangeRates { get; set; } = [];
}