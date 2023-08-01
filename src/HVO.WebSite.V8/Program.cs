
using HVO.WebSite.V8.Repository;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace HVO.WebSite.V8
{
    public static class Program
    {
        public static readonly TimeZoneInfo ObservatoryTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
        public static readonly DateTimeOffset ObservatoryTimeZoneOffset = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Program.ObservatoryTimeZone);

        public static readonly Latitude ObservatoryLatitude = new Latitude(35, 33, 36.1836, CompassPoint.N);
        public static readonly Longitude ObservatoryLongitude = new Longitude(113, 54, 34.1424, CompassPoint.W);


        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            ConfigureApplication(app);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
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
            services.AddSingleton<ITelemetryInitializer>(new TelemetryRoleNameInitializer("HualapaiValleyObservatory-WebSite"));

            services.AddApplicationInsightsTelemetry();
            //services.AddSnapshotCollector((configuration) => Configuration.Bind(nameof(SnapshotCollectorConfiguration), configuration));

            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            // Inline (minimal) API calls
            app.MapGet("/ping", () => { return Results.Ok($"PONG: {DateTimeOffset.Now}"); });
        }
    }

    public class TelemetryRoleNameInitializer : ITelemetryInitializer
    {
        public TelemetryRoleNameInitializer(string roleName)
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