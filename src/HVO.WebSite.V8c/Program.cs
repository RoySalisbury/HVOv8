using HVO.WebSite.V8c.Components;

namespace HVO.WebSite.V8c
{
    public class Program
    {
        public static readonly TimeZoneInfo ObservatoryTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
        public static readonly DateTimeOffset ObservatoryTimeZoneOffset = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, Program.ObservatoryTimeZone);

        //public static readonly Latitude ObservatoryLatitude = new Latitude(35, 33, 36.1836, CompassPoint.N);
        //public static readonly Longitude ObservatoryLongitude = new Longitude(113, 54, 34.1424, CompassPoint.W);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            ConfigureApp(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddOptions();

            services.AddHttpClient("api", client =>
            {
                var apiServerAddress = $"https://hvowebapiv8.azurewebsites.net/api/v1/";
                var apiServerUri = new Uri(apiServerAddress);

                client.BaseAddress = apiServerUri;
                client.DefaultRequestHeaders.Add("accept", "application/json");
            });

            // Add services to the container.
            services.AddRazorComponents()
                .AddServerComponents();
        }

        private static void ConfigureApp(WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.MapRazorComponents<App>().AddServerRenderMode();

            app.MapGet("/ping", () => { return Results.Ok($"PONG: {DateTimeOffset.Now}"); });
        }
    }

}
