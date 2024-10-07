using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.HostedServices;
using Microsoft.AspNetCore.Components;

namespace HVO.WebSite.RoofControlV4.Components.Pages
{
    public partial class Home 
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }

        [Inject]
        IRoofController _roofController { get; set; }

        private string CurrentRoofStatus { get; set; } = RoofControllerStatus.Unknown.ToString();

        public Home() 
        {
        }

        protected override async Task OnInitializedAsync()
        {
            if (this._roofController.IsInitialized == false)
            {
                var returnUrl = this._navigationManager.ToBaseRelativePath(this._navigationManager.BaseUri);
                this._navigationManager.NavigateTo($"/systeminitializing?returnUrl=/{returnUrl}", true);
            }

            await base.OnInitializedAsync();
        }

        private async Task OpenRoof()
        {
            this._roofController.Open();
            this.CurrentRoofStatus = this._roofController.Status.ToString();
        }

        private async Task CloseRoof()
        {
            this._roofController.Close();
            this.CurrentRoofStatus = this._roofController.Status.ToString();
        }

        private async Task StopRoof()
        {
            this._roofController.Stop();
            this.CurrentRoofStatus = this._roofController.Status.ToString();
        }

        private async Task RefreshRoofStatus()
        {
            this.CurrentRoofStatus = this._roofController.Status.ToString();
        }
    }
}
