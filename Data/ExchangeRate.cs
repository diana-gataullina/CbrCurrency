using System.ComponentModel.DataAnnotations.Schema;

namespace CbrCurrency.Data;

public class ExchangeRate
{
    public long Id { get; set; }
    
    public int CurrencyId { get; set; }
    
    public Currency Currency { get; set; } = null!;
    
    public DateOnly Date { get; set; }
    
    [Column(TypeName = "decimal(18, 4)")]
    public decimal Value { get; set; }
}