using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace CurrencyTile.TimerTask.FinancialModelingPrep;

internal class FinancialModelingPrepService
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private readonly ILogger _logger;

    internal FinancialModelingPrepService(ILogger logger)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://financialmodelingprep.com/api/v3/");
        _apiKey = ApiKeys.LoadKey("api_key_financialmodelingprep.txt");
        _logger = logger;
    }

    public async Task<Quote?> GetQuote(string symbol)
    {
        var response = await _client.GetAsync($"quote/{symbol}?apikey={_apiKey}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.Warning(
                $"FinancialModelingPrep call to to /quote/{symbol} failed. Status code: {response?.StatusCode}"
            );
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        List<Quote>? quote = JsonConvert.DeserializeObject<List<Quote>>(json);

        return quote?.FirstOrDefault();
    }
}
