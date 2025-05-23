using HVO.Hardware.RoofControllerV4;
using Microsoft.AspNetCore.Components;

namespace HVO.WebSite.RoofControlV4.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        IRoofController _roofController { get; set; }

        public RoofControllerStatus RoofStatus { get; set; }

        public MainLayout()
        {
        }


        protected async override Task OnInitializedAsync()
        {
            this.RoofStatus = this._roofController.Status;

            this._roofController.PropertyChanged += _roofController_PropertyChanged;
            await base.OnInitializedAsync();
        }

        private void _roofController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RoofStatus = this._roofController.Status;
            InvokeAsync(() => StateHasChanged());
        }

        public void Dispose()
        {
            this._roofController.PropertyChanged -= _roofController_PropertyChanged;
        }


        void NavigationHomeButtonClick()
        {
        }
    }
}
