using CbrCurrency.Data;
using Microsoft.EntityFrameworkCore;

namespace CbrCurrency.Services;

/// <summary>
/// Сервис сохранения курсов в БД.
/// Scoped зависимость, создается на каждую итерацию работы Worker-a
/// </summary>
public class CurrencySyncService(
    AppDbContext dbContext,
    ICbrClient cbrClient,
    ILogger<CurrencySyncService> logger)
{
    public async Task SyncForDateAsync(DateTime date, CancellationToken cancellationToken)
    {
        var data = await cbrClient.GetRatesAsync(date, cancellationToken);
        if (data == null || data.Rates.Count == 0)
        {
            logger.LogWarning("Нет данных от ЦБР на {Date}", date.ToShortDateString());
            return;
        }

        // Проверяем, загружены ли уже данные за ту дату, которую вернул ЦБР
        bool alreadyExists = await dbContext.ExchangeRates
            .AnyAsync(e => e.Date == data.Date, cancellationToken);
            
        if (alreadyExists)
        {
            logger.LogInformation("Данные за {Date} уже существуют в БД. Пропускаем.", data.Date);
            return;
        }

        // Подгружаем существующие валюты, чтобы не создавать дубликатов
        var existingCurrencies = await dbContext.Currencies
            .ToDictionaryAsync(c => c.CbrId, c => c, cancellationToken);

        var ratesToInsert = new List<ExchangeRate>();

        foreach (var rateModel in data.Rates)
        {
            if (!existingCurrencies.TryGetValue(rateModel.CbrId, out var currency))
            {
                currency = new Currency
                {
                    CbrId = rateModel.CbrId,
                    CharCode = rateModel.CharCode,
                    Name = rateModel.Name,
                    Nominal = rateModel.Nominal
                };
                dbContext.Currencies.Add(currency);
                existingCurrencies[rateModel.CbrId] = currency; // кэш для текущего цикла
            }
            else
            {
                currency.Name = rateModel.Name;
                currency.Nominal = rateModel.Nominal;
                currency.CharCode = rateModel.CharCode;
            }

            ratesToInsert.Add(new ExchangeRate
            {
                Currency = currency,
                Date = data.Date,
                Value = rateModel.Value
            });
        }

        dbContext.ExchangeRates.AddRange(ratesToInsert);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Успешно сохранены курсы ({Count} шт.) за {Date}.", ratesToInsert.Count, data.Date);
    }

    /// <summary>
    /// Выполняет первоначальное заполнение БД за последний месяц, если она пуста.
    /// </summary>
    public async Task SyncLastMonthIfNeededAsync(CancellationToken cancellationToken)
    {
        var hasAnyRates = await dbContext.ExchangeRates.AnyAsync(cancellationToken);
        if (hasAnyRates) return;

        logger.LogInformation("БД пуста. Начинаю загрузку за последний месяц...");
        
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-30);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            await SyncForDateAsync(date, cancellationToken);
            // пауза, чтобы не заспамить ЦБР 
            await Task.Delay(500, cancellationToken);
        }
    }
}