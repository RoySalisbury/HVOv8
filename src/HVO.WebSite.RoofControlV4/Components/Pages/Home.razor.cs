using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.HostedServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace HVO.WebSite.RoofControlV4.Components.Pages
{
    public partial class Home 
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }

        [Inject]
        IRoofController _roofController { get; set; }


        bool IsOpenButtonDisabled => this._roofController.Status != RoofControllerStatus.Closed && this._roofController.Status != RoofControllerStatus.Stopped;
        bool IsCloseButtonDisabled => this._roofController.Status != RoofControllerStatus.Open && this._roofController.Status != RoofControllerStatus.Stopped;
        bool IsStopButtonDisabled => false; //this._roofController.Status == RoofControllerStatus.Opening || this._roofController.Status == RoofControllerStatus.Closing;


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

        protected async Task OnOpenRoofClick(MouseEventArgs args)
        {
            this._roofController.Open();
            this.StateHasChanged();
        }

        protected async Task OnCloseRoofClick(MouseEventArgs args)
        {
            this._roofController.Close();
            this.StateHasChanged();
        }

        protected async Task OnStopRoofClick(MouseEventArgs args)
        {
            this._roofController.Stop();

            this.InvokeAsync(() => this.StateHasChanged());
        }
    }
}
