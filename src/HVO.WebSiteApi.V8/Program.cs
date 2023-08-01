using Microsoft.AspNetCore.Mvc;
using HVO.DataModels.HualapaiValleyObservatory;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.ApplicationInsights.Extensibility;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace HVO.WebSiteApi.V8
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

            services.AddDbContext<HualapaiValleyObservatoryDbContext>((s, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("hvo"), sql => sql.MigrationsAssembly(typeof(HualapaiValleyObservatoryDbContext).Assembly.GetName().Name));
            });

            services.AddApiVersioning(setup =>
            {
                setup.ReportApiVersions = true;
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.DefaultApiVersion = new ApiVersion(1, 0);
                setup.ApiVersionReader = new UrlSegmentApiVersionReader(); // ApiVersionReader.Combine(new QueryStringApiVersionReader("version"), new HeaderApiVersionReader("api-version"), new MediaTypeApiVersionReader("version")); 
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
            });

            services.AddProblemDetails(configure =>
            {
            });

            services.AddOutputCache(options =>
            {
                // Basic output caching for general lists of data that will rarely ever change
                //options.AddPolicy(nameof(Controllers.Api.GeneralValuesController.GetGenderList), policy => { policy.Tag(nameof(Controllers.Api.GeneralValuesController.GetGenderList)).Expire(TimeSpan.FromMinutes(5)); });
                //options.AddPolicy(nameof(Controllers.Api.GeneralValuesController.GetEthnicityList), policy => { policy.Tag(nameof(Controllers.Api.GeneralValuesController.GetEthnicityList)).Expire(TimeSpan.FromMinutes(5)); });
            });


            // The following line enables Application Insights telemetry collection.
            services.AddSingleton<ITelemetryInitializer>(new TelemetryRoleNameInitializer("HualapaiValleyObservatory-WebSite-Api"));

            services.AddApplicationInsightsTelemetry();
            //services.AddSnapshotCollector((configuration) => Configuration.Bind(nameof(SnapshotCollectorConfiguration), configuration));

            services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger(options => { });
            app.UseSwaggerUI(options =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            app.MapGet("/ping", () => { return Results.Ok($"PONG: {DateTimeOffset.Now}"); });
            app.MapControllers();
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

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
                return;

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using Bearer tokens. Example: \"Authorization: Bearer {token}\""
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme() { Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                    }
                });



            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "Hualapai Valley Observatory API",
                Version = description.ApiVersion.ToString(),
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}