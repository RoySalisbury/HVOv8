namespace HVO.ObservatoryAgent.DavisVantageProAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(configure =>
            {
                configure.UseStartup<Startup>();
            });
    }
}