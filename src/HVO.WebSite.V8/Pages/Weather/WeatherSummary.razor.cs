using HVO.DataContracts.Weather;
using HVO.Weather;
using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Components;
using System.Threading;

namespace HVO.WebSite.V8.Pages.Weather
{
    public partial class WeatherSummary 
    {
        [Inject]
        private WeatherApiRespository _weatherRespository { get; set; }

        [Parameter]
        public LatestWeatherRecord Model { get; set; } = new LatestWeatherRecord();

        [Parameter]
        public bool DisplayInitialLoader { get; set; } = false;

        private PeriodicTimer _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(15));

        protected override async Task OnInitializedAsync()
        {
            BeginRefresh();

            await base.OnInitializedAsync();
        }

        private async void BeginRefresh()
        {
            while (await this._refreshTimer.WaitForNextTickAsync())
            {
                try
                {
                    this.Model = await this._weatherRespository.GetLatestWeatherRecordHighLow() ?? new LatestWeatherRecord();
                    await InvokeAsync(() => StateHasChanged());
                }
                catch { }
                finally
                {
                    DisplayInitialLoader = false;
                    await InvokeAsync(() => StateHasChanged());
                }
            }
        }

        public void Dispose()
        {
            this._refreshTimer?.Dispose();
        }

        public static string CompassDirection(short? degrees)
        {
            if (degrees.HasValue)
            {
                string[] directions = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };
                return directions[(int)Math.Round(((double)degrees * 10 % 3600) / 225)];
            }
            return "?";
        }


    }
}
