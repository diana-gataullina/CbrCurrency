namespace CbrCurrency.Dto;

public class CbrDailyResponse
{
    public DateOnly Date { get; set; }
    public List<CbrCurrencyRate> Rates { get; set; } = [];
}