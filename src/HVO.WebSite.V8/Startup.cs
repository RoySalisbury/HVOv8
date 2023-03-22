using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Options;
using HVO.WebSite.V8.Repository;

namespace HVO.WebSite.V8
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddScoped<WeatherApiRespository>();

            services.AddHttpClient("api", client =>
            {
                var apiServerAddress = $"https://hvowebapiv8.azurewebsites.net/api/v1/";
                var apiServerUri = new Uri(apiServerAddress);

                client.BaseAddress = apiServerUri;
                client.DefaultRequestHeaders.Add("accept", "application/json");
            });

            // The following line enables Application Insights telemetry collection.
            services.AddSingleton<ITelemetryInitializer>(new TelemetryRoleNameInitilizer("HualapaiValleyObservatory-WebSite"));

            services.AddApplicationInsightsTelemetry();
            //services.AddSnapshotCollector((configuration) => Configuration.Bind(nameof(SnapshotCollectorConfiguration), configuration));

            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");

                // Inline (minimal) API calls
                endpoints.MapGet("/ping", () => { return Results.Ok($"PONG: {DateTimeOffset.Now}"); });
            });
        }
    }

    public class TelemetryRoleNameInitilizer : ITelemetryInitializer
    {
        public TelemetryRoleNameInitilizer(string roleName)
        {
            RoleName = roleName;
        }

        public string RoleName { get; set; }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = RoleName;
            //telemetry.Context.Cloud.RoleInstance = RoleName;
        }
    }
}
