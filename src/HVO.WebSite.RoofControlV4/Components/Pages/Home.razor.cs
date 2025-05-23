using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.HostedServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HVO.WebSite.RoofControlV4.Components.Pages
{
    public partial class Home : IDisposable
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }

        [Inject]
        IRoofController _roofController { get; set; }

        public RoofControllerStatus RoofStatus { get; set; }

        bool IsOpenButtonDisabled => RoofStatus != RoofControllerStatus.Closed && RoofStatus != RoofControllerStatus.Stopped;
        bool IsCloseButtonDisabled => RoofStatus != RoofControllerStatus.Open && RoofStatus != RoofControllerStatus.Stopped;
        bool IsStopButtonDisabled => false; //RoofStatus == RoofControllerStatus.Opening || RoofStatus == RoofControllerStatus.Closing;


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

        protected async Task OnOpenRoofClick(MouseEventArgs args)
        {
            this._roofController.Open();
        }

        protected async Task OnCloseRoofClick(MouseEventArgs args)
        {
            this._roofController.Close();
        }

        protected async Task OnStopRoofClick(MouseEventArgs args)
        {
            this._roofController.Stop();
        }
    }
}
