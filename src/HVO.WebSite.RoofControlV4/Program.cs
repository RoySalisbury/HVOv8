using HVO.Hardware.RoofControllerV4;
using HVO.WebSite.RoofControlV4.Components;
using HVO.WebSite.RoofControlV4.HostedServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.VisualBasic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;

namespace HVO.WebSite.RoofControlV4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // ConfigureServices(IServiceCollection services)
            // ==============================================
            builder.Services.AddOptions();
            builder.Services.Configure<RoofControllerOptions>(builder.Configuration.GetSection(nameof(RoofControllerOptions)));
            builder.Services.Configure<RoofControllerHostOptions>(builder.Configuration.GetSection(nameof(RoofControllerHostOptions)));

            builder.Services.AddSingleton<IRoofController, RoofController>();
            builder.Services.AddHostedService<RoofControllerHost>();

            builder.Services.AddApiVersioning(setup =>
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


            builder.Services.AddProblemDetails(configure =>
            {
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v4", new OpenApiInfo { Title = "HVO Roof Control API", Version = "v4.0", Description = "Controls the observatory roof for HVO." });
            });

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure(IApplicationBuilder app)
            //===================================

            // Configure the HTTP request pipeline.
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

            app.Run();
        }
    }
}