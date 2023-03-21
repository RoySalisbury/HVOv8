using HVO.Weather.DavisVantagePro;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.NotificationServices
{
    public interface IWeatherUpdateNotificationService
    {
        event EventHandler<DavisVantageProConsoleRecord> OnWeatherRecordReceived;
        DavisVantageProConsoleRecord LatestRecord { get; }

    }

    public class WeatherUpdateNotificationService : IWeatherUpdateNotificationService
    {
        public event EventHandler<DavisVantageProConsoleRecord> OnWeatherRecordReceived;

        public DavisVantageProConsoleRecord LatestRecord { get; private set; }

        public void Update(DavisVantageProConsoleRecord weatherRecord)
        {
            LatestRecord = weatherRecord;
            OnWeatherRecordReceived?.Invoke(this, weatherRecord);
        }
    }
}
