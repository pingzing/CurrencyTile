using System.Diagnostics;
using Newtonsoft.Json;

namespace CurrencyTile.TimerTask.CurrencyBeacon;

internal class CurrencyBeaconService
{
    private HttpClient _client;
    private string _apiKey;

    internal CurrencyBeaconService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api.currencybeacon.com/v1/");
        _apiKey = ApiKeys.LoadKey("api_key_currencybeacon.txt");
    }

    public async Task<ExchangeRate?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        var response = await _client.GetAsync(
            $"convert?from={fromCurrency}&to={toCurrency}&amount=1&api_key={_apiKey}"
        );
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine(
                $"CurrencyBeacon call to /convert?from={fromCurrency}&to={toCurrency}&amount=1 failed. Status code: {response?.StatusCode}"
            );
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        ExchangeRate? rate = JsonConvert.DeserializeObject<ExchangeRate>(json);

        return rate;
    }
}
