using CurrencyTile.TimerTask.Models;
using Newtonsoft.Json;

namespace CurrencyTile.TimerTask;

internal interface IAlphaVantageService
{
    Task<GlobalQuote?> GetGlobalQuote(string symbol);
    Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency);
}

internal class AlphaVantageService : IAlphaVantageService
{
    private HttpClient _client;
    private string _apiKey;

    internal AlphaVantageService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://www.alphavantage.co/");
        Stream? apiKeyStream = typeof(AlphaVantageService).Assembly.GetManifestResourceStream(
            "CurrencyTile.TimerTask.api_key.txt"
        );
        if (apiKeyStream == null)
        {
            throw new Exception(
                "Missing the api_key.txt file that's supposed to contain an API key in the background task project. Go create it!\n"
                    + "(It needs to be an embedded resource, btw!)"
            );
        }
        using TextReader reader = new StreamReader(apiKeyStream);
        string apiKey = reader.ReadToEnd();
        if (String.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception(
                "API key read from the file is null, empty, or whitespace. Fix it!"
            );
        }
        _apiKey = apiKey;
    }

    // TODO: Stub this and the one below to work around the TEENY TINY API limits that AlphaVantage imposes
    public async Task<GlobalQuote?> GetGlobalQuote(string symbol)
    {
        var response = await _client.GetAsync(
            $"query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}"
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        GlobalQuoteWrapper? quoteWrapper = JsonConvert.DeserializeObject<GlobalQuoteWrapper>(json);
        if (quoteWrapper == null)
        {
            return null;
        }

        return quoteWrapper.GlobalQuote;
    }

    public async Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        var response = await _client.GetAsync(
            $"query?function=CURRENCY_EXCHANGE_RATE&from_currency={fromCurrency}&to_currency={toCurrency}&apikey={_apiKey}"
        );
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        ExchangeRateWrapper? erWrapper = JsonConvert.DeserializeObject<ExchangeRateWrapper>(json);
        if (erWrapper == null)
        {
            return null;
        }

        return erWrapper.RealtimeCurrencyExchangeRate;
    }
}

internal class DummyAlphaVantageService : IAlphaVantageService
{
    public Task<GlobalQuote?> GetGlobalQuote(string symbol)
    {
        // TODO: some JSON, yo
        return Task.FromResult<GlobalQuote?>(null);
    }

    public Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        // TODO: some JSON, yo
        return Task.FromResult<ExchangeRate?>(null);
    }
}
