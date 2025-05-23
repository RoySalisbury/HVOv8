using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.Components;
using HVO.WebSite.RoofControlV4.HostedServices;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.OpenApi.Models;
using Radzen;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace HVO.WebSite.RoofControlV4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();
            Configure(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager Configuration)
        {
            services.AddOptions();
            services.Configure<RoofControllerOptions>(Configuration.GetSection(nameof(RoofControllerOptions)));
            services.Configure<RoofControllerHostOptions>(Configuration.GetSection(nameof(RoofControllerHostOptions)));

            services.AddSingleton<IRoofController, RoofController>();
            services.AddHostedService<RoofControllerHost>();

            services.AddApiVersioning(setup =>
            {
                setup.DefaultApiVersion = new ApiVersion(4, 0);
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.ReportApiVersions = true;
                setup.ApiVersionReader = new UrlSegmentApiVersionReader(); // ApiVersionReader.Combine(new QueryStringApiVersionReader("version"), new HeaderApiVersionReader("api-version"), new MediaTypeApiVersionReader("version")); 
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });


            services.AddProblemDetails(configure =>
            {
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v4", new OpenApiInfo { Title = "HVO Roof Control API", Version = "v4.0", Description = "Controls the observatory roof for HVO." });
            });

            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddRadzenComponents();
            services.AddRadzenCookieThemeService(options =>
            {
                options.Name = "Test1Theme";
                options.Duration = TimeSpan.FromDays(365);
            });
            services.AddHttpClient();
        }



        private static void Configure(WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var descriptions = app.DescribeApiVersions();
                foreach (var description in descriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        }
    }
}