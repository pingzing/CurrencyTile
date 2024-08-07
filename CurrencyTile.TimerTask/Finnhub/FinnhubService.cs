using System.Diagnostics;
using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.Finnhub;

internal class FinnhubService
{
    private HttpClient _client;
    private string _apiKey;

    internal FinnhubService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://finnhub.io/api/v1/");
        _apiKey = ApiKeys.LoadKey("api_key_finnhub.txt");
    }

    public async Task<StockQuote?> GetQuote(string symbol)
    {
        var response = await _client.GetAsync($"quote?symbol={symbol}&token={_apiKey}");
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine(
                $"Finnhub call to ${response?.RequestMessage?.RequestUri} failed. Status code: {response?.StatusCode}"
            );
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        StockQuote? quote = JsonConvert.DeserializeObject<StockQuote>(json);
        if (quote != null)
        {
            // Not returned as part of the API response, so we do it ourselves
            quote.Symbol = symbol;
        }
        return quote;
    }
}
