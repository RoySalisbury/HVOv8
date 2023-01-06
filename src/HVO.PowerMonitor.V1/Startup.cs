using HVO.PowerMonitor.V1.HostedServices.BatteryService;
using HVO.PowerMonitor.V1.HostedServices.InverterService;
using Microsoft.Extensions.Options;

namespace HVO.PowerMonitor.V1
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddOptions<InverterClientOptions>();
            services.AddOptions<InverterServiceProcessorOptions>();

            services.AddTransient<InverterCommunicationsClient>(serviceProvider => 
            {
                var logger = serviceProvider.GetService<ILogger<InverterCommunicationsClient>>();
                var options = serviceProvider.GetService<IOptions<InverterClientOptions>>();

                return InverterCommunicationsClient.Create(logger, options.Value);
            });

            services.AddSingleton<IInverterServiceProcessor, InverterServiceProcessor>();
            services.AddHostedService<InverterServiceHost>();

            services.AddSingleton<IBatteryManagerController, BatteryManagerController>();
            services.AddHostedService<BatteryManagerControllerHost>();


            // Add services to the container.
            services.AddRazorPages();
            services.AddServerSideBlazor(options => {
                options.DetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // Configure the HTTP request pipeline.
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");

                endpoints.MapGet("/ping", () => { return Results.Ok($"PONG: {DateTimeOffset.Now}"); });
            });
        }
    }
}
