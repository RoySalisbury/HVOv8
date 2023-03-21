using HVO.DataModels.HualapaiValleyObservatory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Transactions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, configure) => 
    {
        configure.AddDbContext<HualapaiValleyObservatoryDbContext>(options =>
        {
            options.UseSqlServer("Server=hvo.database.windows.net;Database=HualapaiValleyObservatory;User Id=roys;Password=1qaz!qaz", a =>
            {
                a.EnableRetryOnFailure();
            });
        });
    })
    .Build();

host.Run();
