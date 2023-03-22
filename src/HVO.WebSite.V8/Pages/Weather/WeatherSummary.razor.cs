using HVO.WebSite.V8.DataContracts.Weather;
using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Components;
using System.Threading;

namespace HVO.WebSite.V8.Pages.Weather
{
    public partial class WeatherSummary 
    {
        [Inject]
        private WeatherRespository _weatherRespository { get; set; }

        [Parameter]
        public LatestWeatherRecord Model { get; set; } = new LatestWeatherRecord();

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
                    Model = await this._weatherRespository.GetLatestWeatherRecordHighLow();
                    await InvokeAsync(() => StateHasChanged());
                }
                catch { }
                finally
                {
                }
            }
        }

        public void Dispose()
        {
            this._refreshTimer?.Dispose();
        }
    }
}
