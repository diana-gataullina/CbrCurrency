namespace CbrCurrency.Dto;

public class CbrCurrencyRate
{
    public string CbrId { get; set; } = null!;
    public string CharCode { get; set; } = null!;
    public int Nominal { get; set; }
    public string Name { get; set; } = null!;
    public decimal Value { get; set; }
}