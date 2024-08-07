using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.AlphaVantage;

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

        _apiKey = ApiKeys.LoadKey("api_key_alphavantage.txt");
    }

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
    private readonly Random _random = new();

    public Task<GlobalQuote?> GetGlobalQuote(string symbol)
    {
        double variance = _random.Next(-5, 5) + _random.NextDouble();
        string json =
            $@"{{
        ""01. symbol"": ""VFFVX"",
        ""02. open"": ""52.2100"",
        ""03. high"": ""52.2100"",
        ""04. low"": ""52.2100"",
        ""05. price"": ""{55.0495m + (decimal)variance}"",
        ""06. volume"": ""0"",
        ""07. latest trading day"": ""{DateOnly.FromDateTime(DateTime.Now):O}"",
        ""08. previous close"": ""53.5500"",
        ""09. change"": ""-1.3400"",
        ""10. change percent"": ""-2.5023%""
    }}";
        return Task.FromResult(JsonConvert.DeserializeObject<GlobalQuote>(json));
    }

    public Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        // TODO: some JSON, yo
        return Task.FromResult<ExchangeRate?>(null);
    }
}
