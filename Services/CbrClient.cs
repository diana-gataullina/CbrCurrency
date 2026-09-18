using System.Globalization;
using System.Xml.Linq;
using CbrCurrency.Dto;

namespace CbrCurrency.Services;

public class CbrClient(HttpClient httpClient) : ICbrClient
{
    private const string BaseUrl = "https://www.cbr.ru/scripts/XML_daily.asp";

    public async Task<CbrDailyResponse?> GetRatesAsync(DateTime date, CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}?date_req={date:dd/MM/yyyy}";
        
        await using var stream = await httpClient.GetStreamAsync(url, cancellationToken);
        var xml = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var root = xml.Root;
        if (root == null) return null;

        // Парсим дату из атрибута Date, тк при запросе за выходной день
        // ЦБР возвращает XML с датой пятницы/субботы.
        var dateStr = root.Attribute("Date")?.Value;
        if (!DateOnly.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var actualDate))
            return null;

        var response = new CbrDailyResponse { Date = actualDate };
        var ruCulture = new CultureInfo("ru-RU"); 

        foreach (var element in root.Elements("Valute"))
        {
            response.Rates.Add(new CbrCurrencyRate
            {
                CbrId = element.Attribute("ID")?.Value ?? string.Empty,
                CharCode = element.Element("CharCode")?.Value ?? string.Empty,
                Name = element.Element("Name")?.Value ?? string.Empty,
                Nominal = int.Parse(element.Element("Nominal")?.Value ?? "1"),
                Value = decimal.Parse(element.Element("Value")?.Value ?? "0", ruCulture)
            });
        }

        return response;
    }
}