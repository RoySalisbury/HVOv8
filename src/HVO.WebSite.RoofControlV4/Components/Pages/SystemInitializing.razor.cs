using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.HostedServices;
using Microsoft.AspNetCore.Components;

namespace HVO.WebSite.RoofControlV4.Components.Pages
{
    public partial class SystemInitializing
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }

        [Inject]
        IRoofController _roofController { get; set; }

        [SupplyParameterFromQuery(Name = "ReturnUrl")]
        public string ReturnUrl { get; set; } = "/";


        protected override async void OnAfterRender(bool firstRender)
        {
            if (firstRender) 
            {
                using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await periodicTimer.WaitForNextTickAsync())
                {
                    if (this._roofController.IsInitialized == true)
                    {
                        //await InvokeAsync(() => { this._navigationManager.NavigateTo(this.ReturnUrl ?? "/", true, true); });
                        this._navigationManager.NavigateTo(this.ReturnUrl ?? "/", true, true);
                        break;
                    }
                }
            }

            base.OnAfterRender(firstRender);
        }
    }
}
