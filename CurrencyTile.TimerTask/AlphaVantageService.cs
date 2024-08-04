using CurrencyTile.TimerTask.Models;
using Newtonsoft.Json;

namespace CurrencyTile.TimerTask
{
    internal class AlphaVantageService
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
                    "Missing the api_key.txt file that's supposed to contain an API key in the background task project. Go create it!"
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

        internal async Task<GlobalQuote?> GetGlobalQuote(string symbol)
        {
            var response = await _client.GetAsync(
                $"query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}"
            );
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            GlobalQuoteWrapper? quoteWrapper = JsonConvert.DeserializeObject<GlobalQuoteWrapper>(
                json
            );
            if (quoteWrapper == null)
            {
                return null;
            }

            return quoteWrapper.GlobalQuote;
        }

        internal async Task GetExchangeRate(string fromCurrency, string toCurrency)
        {
            throw new NotImplementedException();
        }
    }
}
