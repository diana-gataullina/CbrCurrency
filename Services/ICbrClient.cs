using CbrCurrency.Dto;

namespace CbrCurrency.Services;

public interface ICbrClient
{
    Task<CbrDailyResponse?> GetRatesAsync(DateTime date, CancellationToken cancellationToken);
}