using HVO.DataContracts.Weather;
using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Components;
using System.Data.SqlTypes;

namespace HVO.WebSite.V8.Pages.Weather
{
    public partial class WeatherIndex
    {
        [Inject]
        private WeatherApiRespository _weatherRespository { get; set; }

        [Parameter]
        public LatestWeatherRecord Model { get; set; } = new LatestWeatherRecord();

        public bool DisplaySummaryLoader { get; set; } = true;

        public bool DisplayAstroInformationLoader { get; set; } = true;
       

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadLatestWeatherRecordHighLow();
                await LoadAstroInformation();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task LoadLatestWeatherRecordHighLow()
        {
            try
            {
                this.Model = await this._weatherRespository.GetLatestWeatherRecordHighLow() ?? new LatestWeatherRecord();
                await this.InvokeAsync(this.StateHasChanged);
            }
            finally
            {
                DisplaySummaryLoader = false;
            }

            await this.InvokeAsync(this.StateHasChanged);
        }

        private async Task LoadAstroInformation()
        {
            try
            {
                //this.Model = await this._weatherRespository.GetLatestWeatherRecordHighLow() ?? new LatestWeatherRecord();
                //await this.InvokeAsync(this.StateHasChanged);
            }
            finally
            {
                DisplayAstroInformationLoader = false;
            }

            await this.InvokeAsync(this.StateHasChanged);
        }

    }
}
