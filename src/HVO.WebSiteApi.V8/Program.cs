
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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}