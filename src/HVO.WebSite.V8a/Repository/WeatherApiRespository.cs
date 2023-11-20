using HVO.DataContracts.Weather;

namespace HVO.WebSite.V8a.Repository
{
    public sealed class WeatherApiRespository
    {
        private readonly ILogger<WeatherApiRespository> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherApiRespository(ILogger<WeatherApiRespository> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LatestWeatherRecord?> GetLatestWeatherRecordHighLow(CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"weather/GetLatestWeatherRecordHighLow");
            var client = _httpClientFactory.CreateClient("api");

            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LatestWeatherRecord>(cancellationToken: cancellationToken);
            }

            return null;
        }

        public async Task<dynamic?> GetLatestWeatherRecord(CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"weather/GetLatestWeatherRecord");
            var client = _httpClientFactory.CreateClient("api");

            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
            }

            return null;
        }

        public async Task<dynamic?> GetDavisVantageProOneMinuteAverage(DateTimeOffset startDateTime, DateTimeOffset endDateTime, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"weather/GetDavisVantageProOneMinuteAverage?startDateTime={startDateTime}&endDateTime={endDateTime}");
            var client = _httpClientFactory.CreateClient("api");

            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
            }

            return null;
        }
    }
}
