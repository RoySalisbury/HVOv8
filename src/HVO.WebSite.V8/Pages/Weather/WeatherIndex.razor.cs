using HVO.DataContracts.Weather;
using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Components;

namespace HVO.WebSite.V8.Pages.Weather
{
    public partial class WeatherIndex
    {
        [Inject]
        private WeatherApiRespository _weatherRespository { get; set; }

        [Parameter]
        public LatestWeatherRecord Model { get; set; } = new LatestWeatherRecord();

        protected override async Task OnInitializedAsync()
        {
            this.Model = await this._weatherRespository.GetLatestWeatherRecordHighLow();
            this.Model ??= new LatestWeatherRecord();

            await base.OnInitializedAsync();
        }
    }
}
