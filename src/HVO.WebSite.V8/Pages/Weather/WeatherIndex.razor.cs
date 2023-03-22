using HVO.WebSite.V8.DataContracts.Weather;
using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Client;

namespace HVO.WebSite.V8.Pages.Weather
{
    public partial class WeatherIndex
    {
        [Inject]
        private WeatherRespository _weatherRespository { get; set; }

        [Parameter]
        public LatestWeatherRecord Model { get; set; } = new LatestWeatherRecord();

        protected override async Task OnInitializedAsync()
        {
            Model = await this._weatherRespository.GetLatestWeatherRecordHighLow();

            await base.OnInitializedAsync();
        }
    }
}
