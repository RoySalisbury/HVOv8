using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.Pages.Shared
{
    public partial class RedirectToLogin
    {
        [Inject]
        private NavigationManager _navigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var returnUrl = this._navigationManager.ToBaseRelativePath(this._navigationManager.Uri);
            this._navigationManager.NavigateTo($"Login?returnUrl={returnUrl}", true);

            await base.OnInitializedAsync();
        }
    }
}
