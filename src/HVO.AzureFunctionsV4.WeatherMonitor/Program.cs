using HVO.DataModels.HualapaiValleyObservatory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Transactions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(o => 
    {
        o.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) => 
    {
        services.AddDbContext<HualapaiValleyObservatoryDbContext>((s, options) =>
        {
            options.UseSqlServer(context.Configuration.GetConnectionString("hvo"), sql =>
            {
                sql.MigrationsAssembly(typeof(HualapaiValleyObservatoryDbContext).Assembly.GetName().Name);
                sql.EnableRetryOnFailure();
            });
        });
    })
    .Build();

host.Run();
