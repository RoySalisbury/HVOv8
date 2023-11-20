using HVO.WebSite.V8a.Components;
using HVO.WebSite.V8a.Repository;
using Radzen;

namespace HVO.WebSite.V8a
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
            AddServices(builder.Configuration, builder.Services);

            var app = builder.Build();
            ConfigureApp(builder.Environment, app);

            app.Run();
        }

        private static void AddServices(ConfigurationManager configuration, IServiceCollection services)
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

            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddRadzenComponents();
        }

        private static void ConfigureApp(IWebHostEnvironment environment, WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        }
    }
}