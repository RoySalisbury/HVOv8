using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HVO.DataModels.HualapaiValleyObservatory;

public partial class HualapaiValleyObservatoryDbContext : DbContext
{
    public HualapaiValleyObservatoryDbContext()
    {
    }

    public HualapaiValleyObservatoryDbContext(DbContextOptions<HualapaiValleyObservatoryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AllSkyCameraRecord> AllSkyCameraRecords { get; set; }
    public virtual DbSet<DavisVantageProConsoleRecord> DavisVantageProConsoleRecords { get; set; }

    public virtual DbSet<DavisVantageProConsoleRecordsOneMinuteArchive> DavisVantageProConsoleRecordsOneMinuteArchives { get; set; }
    public virtual DbSet<OutbackMateChargeControllerRecord> OutbackMateChargeControllerRecords { get; set; }
    public virtual DbSet<OutbackMateChargeControllerRecordsOneMinuteArchive> OutbackMateChargeControllerRecordsOneMinuteArchives { get; set; }
    public virtual DbSet<OutbackMateFlexNetRecord> OutbackMateFlexNetRecords { get; set; }
    public virtual DbSet<OutbackMateFlexNetRecordsOneMinuteArchive> OutbackMateFlexNetRecordsOneMinuteArchives { get; set; }
    public virtual DbSet<OutbackMateInverterChargerRecord> OutbackMateInverterChargerRecords { get; set; }
    public virtual DbSet<OutbackMateInverterChargerRecordsOneMinuteArchive> OutbackMateInverterChargerRecordsOneMinuteArchives { get; set; }
    public virtual DbSet<SecurityCameraRecord> SecurityCameraRecords { get; set; }

    public virtual DbSet<CameraRecord> CameraRecords { get; set; }

    public virtual DbSet<SkyMonitor> SkyMonitors { get; set; }
    public virtual DbSet<WeatherCameraRecord> WeatherCameraRecords { get; set; }
    public virtual DbSet<WeatherSatelliteRecord> WeatherSatelliteRecords { get; set; }
    public virtual DbSet<WebPowerSwitchConfiguration> WebPowerSwitchConfigurations { get; set; }

    private DbSet<RawModels.WeatherRecordHighLowSummary> WeatherRecordHighLowSummary { get; set; }
    //        private DbSet<RawModels.OutbackFlexNetShuntAverage> OutbackFlexNetShuntAverage { get; set; }
    private DbSet<RawModels.DavisVantageProAverage> DavisVantageProAverage { get; set; }





    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=hvo.database.windows.net;Database=HualapaiValleyObservatory;User Id=roys;Password=1qaz!qaz");
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AllSkyCameraRecord>(entity =>
        {
            entity.HasIndex(x => new { x.RecordDateTime, x.CameraNumber, x.ImageType }/*, "IX_AllSkyCameraRecords"*/);

            entity.Property(e => e.StorageLocation)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<DavisVantageProConsoleRecord>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("DavisVantageProConsoleRecords_");

            entity.Property(e => e.Barometer)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("barometer")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BarometerTrend).HasColumnName("barometerTrend");

            entity.Property(e => e.ConsoleBatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("consoleBatteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ForcastIcons).HasColumnName("forcastIcons");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.InsideHumidity).HasColumnName("insideHumidity");

            entity.Property(e => e.InsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("insideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideDewpoint)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideDewpoint")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHeatIndex)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideHeatIndex")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHumidity).HasColumnName("outsideHumidity");

            entity.Property(e => e.OutsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideWindChill)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideWindChill")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RainRate)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("rainRate")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SolarRadiation).HasColumnName("solarRadiation");

            entity.Property(e => e.StormRain)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("stormRain")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.StormStartDate).HasColumnName("stormStartDate");

            entity.Property(e => e.SunriseTime).HasColumnName("sunriseTime");

            entity.Property(e => e.SunsetTime).HasColumnName("sunsetTime");

            entity.Property(e => e.TenMinuteWindSpeedAverage).HasColumnName("tenMinuteWindSpeedAverage");

            entity.Property(e => e.UvIndex).HasColumnName("uvIndex");

            entity.Property(e => e.WindDirection).HasColumnName("windDirection");

            entity.Property(e => e.WindSpeed).HasColumnName("windSpeed");

            entity.Property(e => e.YearlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.YearlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");
        });

        modelBuilder.Entity<DavisVantageProConsoleRecordNew>(entity =>
        {
            entity.ToTable("DavisVantageProConsoleRecords_NEW");

            entity.HasIndex(x => x.RecordDateTime/*, "IX_DavisVantageProConsoleRecords_NEW"*/)
                .IsUnique();

            entity.HasIndex(x => new { x.Barometer, x.InsideTemperature, x.InsideHumidity, x.OutsideTemperature, x.OutsideHumidity, x.WindSpeed, x.WindDirection, x.RainRate, x.UvIndex, x.SolarRadiation, x.StormRain, x.StormStartDate, x.DailyRainAmount, x.MonthlyRainAmount, x.YearlyRainAmount, x.ConsoleBatteryVoltage, x.SunriseTime, x.SunsetTime, x.DailyEtamount, x.MonthlyEtamount, x.YearlyEtamount, x.OutsideHeatIndex, x.OutsideWindChill, x.OutsideDewpoint, x.Id, x.BarometerTrend, x.TenMinuteWindSpeedAverage, x.ForcastIcons, x.RecordDateTime }/*, "IX_DavisVantageProConsoleRecords_NEW_Cover"*/)
                .IsUnique();

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Barometer)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("barometer")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BarometerTrend).HasColumnName("barometerTrend");

            entity.Property(e => e.ConsoleBatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("consoleBatteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ForcastIcons).HasColumnName("forcastIcons");

            entity.Property(e => e.InsideHumidity).HasColumnName("insideHumidity");

            entity.Property(e => e.InsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("insideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideDewpoint)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideDewpoint")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHeatIndex)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideHeatIndex")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHumidity).HasColumnName("outsideHumidity");

            entity.Property(e => e.OutsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideWindChill)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideWindChill")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RainRate)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("rainRate")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SolarRadiation).HasColumnName("solarRadiation");

            entity.Property(e => e.StormRain)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("stormRain")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.StormStartDate).HasColumnName("stormStartDate");

            entity.Property(e => e.SunriseTime).HasColumnName("sunriseTime");

            entity.Property(e => e.SunsetTime).HasColumnName("sunsetTime");

            entity.Property(e => e.TenMinuteWindSpeedAverage).HasColumnName("tenMinuteWindSpeedAverage");

            entity.Property(e => e.UvIndex).HasColumnName("uvIndex");

            entity.Property(e => e.WindDirection).HasColumnName("windDirection");

            entity.Property(e => e.WindSpeed).HasColumnName("windSpeed");

            entity.Property(e => e.YearlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.YearlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");
        });

        modelBuilder.Entity<DavisVantageProConsoleRecordsOneMinuteArchive>(entity =>
        {
            entity.ToTable("DavisVantageProConsoleRecords_OneMinuteArchive");

            entity.HasIndex(x => x.RecordDateTime/*, "IX_DavisVantageProConsoleRecords_OneMinuteArchive_RecordDateTime"*/)
                .IsUnique();

            entity.HasIndex(x => new { x.Barometer, x.InsideTemperature, x.InsideHumidity, x.OutsideTemperature, x.OutsideHumidity, x.WindSpeed, x.WindDirection, x.RainRate, x.UvIndex, x.SolarRadiation, x.StormRain, x.StormStartDate, x.DailyRainAmount, x.MonthlyRainAmount, x.YearlyRainAmount, x.ConsoleBatteryVoltage, x.SunriseTime, x.SunsetTime, x.DailyEtamount, x.MonthlyEtamount, x.YearlyEtamount, x.OutsideHeatIndex, x.OutsideWindChill, x.OutsideDewpoint, x.RecordDateTime }/*, "IX_DavisVantageProConsoleRecords_OneMinuteArchive_RecordDateTime_Cover"*/);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Barometer)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("barometer")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BarometerTrend).HasColumnName("barometerTrend");

            entity.Property(e => e.ConsoleBatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("consoleBatteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("dailyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ForcastIcons).HasColumnName("forcastIcons");

            entity.Property(e => e.InsideHumidity).HasColumnName("insideHumidity");

            entity.Property(e => e.InsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("insideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.MonthlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("monthlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideDewpoint)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideDewpoint")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHeatIndex)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideHeatIndex")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideHumidity).HasColumnName("outsideHumidity");

            entity.Property(e => e.OutsideTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideTemperature")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.OutsideWindChill)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("outsideWindChill")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RainRate)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("rainRate")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SolarRadiation).HasColumnName("solarRadiation");

            entity.Property(e => e.StormRain)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("stormRain")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.StormStartDate).HasColumnName("stormStartDate");

            entity.Property(e => e.SunriseTime).HasColumnName("sunriseTime");

            entity.Property(e => e.SunsetTime).HasColumnName("sunsetTime");

            entity.Property(e => e.TenMinuteWindSpeedAverage).HasColumnName("tenMinuteWindSpeedAverage");

            entity.Property(e => e.UvIndex).HasColumnName("uvIndex");

            entity.Property(e => e.WindDirection).HasColumnName("windDirection");

            entity.Property(e => e.WindSpeed).HasColumnName("windSpeed");

            entity.Property(e => e.YearlyEtamount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyETAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.YearlyRainAmount)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("yearlyRainAmount")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");
        });

        modelBuilder.Entity<OutbackMateChargeControllerRecord>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("OutbackMateChargeControllerRecords_");

            entity.Property(e => e.ChargerAmps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargerAuxRelayMode).HasColumnName("chargerAuxRelayMode");

            entity.Property(e => e.ChargerErrorMode).HasColumnName("chargerErrorMode");

            entity.Property(e => e.ChargerMode).HasColumnName("chargerMode");

            entity.Property(e => e.ChargerVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyAmpHoursProduced).HasColumnName("dailyAmpHoursProduced");

            entity.Property(e => e.DailyWattHoursProduced).HasColumnName("dailyWattHoursProduced");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.PvAmps).HasColumnName("pvAmps");

            entity.Property(e => e.PvVoltage).HasColumnName("pvVoltage");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");
        });

        modelBuilder.Entity<OutbackMateChargeControllerRecordNew>(entity =>
        {
            entity.ToTable("OutbackMateChargeControllerRecords_NEW");

            entity.HasIndex(x => new { x.HubPort, x.RecordDateTime }/*, "IX_OutbackMateChargeControllerRecords_cover01"*/);

            entity.HasKey(x => x.Id)
                .HasName("PK_OutbackMateChargeControllerRecord")
                .IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ChargerAmps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargerAuxRelayMode).HasColumnName("chargerAuxRelayMode");

            entity.Property(e => e.ChargerErrorMode).HasColumnName("chargerErrorMode");

            entity.Property(e => e.ChargerMode).HasColumnName("chargerMode");

            entity.Property(e => e.ChargerVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyAmpHoursProduced).HasColumnName("dailyAmpHoursProduced");

            entity.Property(e => e.DailyWattHoursProduced).HasColumnName("dailyWattHoursProduced");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.PvAmps).HasColumnName("pvAmps");

            entity.Property(e => e.PvVoltage).HasColumnName("pvVoltage");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");
        });

        modelBuilder.Entity<OutbackMateChargeControllerRecordsOneMinuteArchive>(entity =>
        {
            entity.ToTable("OutbackMateChargeControllerRecords_OneMinuteArchive");

            entity.HasIndex(x => x.RecordDateTime/*, "IX_OutbackMateChargeControllerRecords_OneMinuteArchive_RecordDateTime"*/);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ChargerAmps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargerAuxRelayMode).HasColumnName("chargerAuxRelayMode");

            entity.Property(e => e.ChargerErrorMode).HasColumnName("chargerErrorMode");

            entity.Property(e => e.ChargerMode).HasColumnName("chargerMode");

            entity.Property(e => e.ChargerVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("chargerVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.DailyAmpHoursProduced).HasColumnName("dailyAmpHoursProduced");

            entity.Property(e => e.DailyWattHoursProduced).HasColumnName("dailyWattHoursProduced");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.PvAmps).HasColumnName("pvAmps");

            entity.Property(e => e.PvVoltage).HasColumnName("pvVoltage");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");
        });

        modelBuilder.Entity<OutbackMateFlexNetRecord>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("OutbackMateFlexNetRecords_");

            entity.Property(e => e.BatteryStateOfCharge).HasColumnName("batteryStateOfCharge");

            entity.Property(e => e.BatteryTemperatureC).HasColumnName("batteryTemperatureC");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargeParamsMet).HasColumnName("chargeParamsMet");

            entity.Property(e => e.ExtraValue)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("extraValue")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ExtraValueTypeId).HasColumnName("extraValueTypeId");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.RelayMode).HasColumnName("relayMode");

            entity.Property(e => e.RelayState).HasColumnName("relayState");

            entity.Property(e => e.ShuntAamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntAAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntAenabled).HasColumnName("shuntAEnabled");

            entity.Property(e => e.ShuntBamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntBAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntBenabled).HasColumnName("shuntBEnabled");

            entity.Property(e => e.ShuntCamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntCAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntCenabled).HasColumnName("shuntCEnabled");
        });

        modelBuilder.Entity<OutbackMateFlexNetRecordNew>(entity =>
        {
            entity.HasKey(x => x.Id)
                .HasName("PK_OutbackMateFlexNetRecord")
                .IsClustered(false);

            entity.ToTable("OutbackMateFlexNetRecords_NEW");

            entity.HasIndex(x => new { x.ShuntAamps, x.ShuntBamps, x.ShuntCamps, x.BatteryVoltage, x.BatteryStateOfCharge, x.BatteryTemperatureC, x.ChargeParamsMet, x.ShuntAenabled, x.ShuntBenabled, x.ShuntCenabled, x.RecordDateTime }/*, "IX_OutbackMateFlexNetRecords_cover01"*/);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.BatteryStateOfCharge).HasColumnName("batteryStateOfCharge");

            entity.Property(e => e.BatteryTemperatureC).HasColumnName("batteryTemperatureC");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargeParamsMet).HasColumnName("chargeParamsMet");

            entity.Property(e => e.ExtraValue)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("extraValue")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ExtraValueTypeId).HasColumnName("extraValueTypeId");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.RelayMode).HasColumnName("relayMode");

            entity.Property(e => e.RelayState).HasColumnName("relayState");

            entity.Property(e => e.ShuntAamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntAAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntAenabled).HasColumnName("shuntAEnabled");

            entity.Property(e => e.ShuntBamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntBAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntBenabled).HasColumnName("shuntBEnabled");

            entity.Property(e => e.ShuntCamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntCAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntCenabled).HasColumnName("shuntCEnabled");
        });

        modelBuilder.Entity<OutbackMateFlexNetRecordsOneMinuteArchive>(entity =>
        {
            entity.ToTable("OutbackMateFlexNetRecords_OneMinuteArchive");

            entity.HasIndex(x => new { x.HubPort, x.ShuntAamps, x.ShuntBamps, x.ShuntCamps, x.BatteryVoltage, x.BatteryStateOfCharge, x.BatteryTemperatureC, x.ChargeParamsMet, x.ShuntAenabled, x.ShuntBenabled, x.ShuntCenabled, x.ExtraValueTypeId, x.ExtraValue, x.RelayState, x.RelayMode, x.RecordDateTime }/*, "IX_OutbackMateFlexNetRecords_OneMinuteArchive_Cover"*/);

            entity.HasIndex(x => x.RecordDateTime/*, "IX_OutbackMateFlexNetRecords_OneMinuteArchive_RecordDateTime"*/);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.BatteryStateOfCharge).HasColumnName("batteryStateOfCharge");

            entity.Property(e => e.BatteryTemperatureC).HasColumnName("batteryTemperatureC");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ChargeParamsMet).HasColumnName("chargeParamsMet");

            entity.Property(e => e.ExtraValue)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("extraValue")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ExtraValueTypeId).HasColumnName("extraValueTypeId");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.RelayMode).HasColumnName("relayMode");

            entity.Property(e => e.RelayState).HasColumnName("relayState");

            entity.Property(e => e.ShuntAamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntAAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntAenabled).HasColumnName("shuntAEnabled");

            entity.Property(e => e.ShuntBamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntBAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntBenabled).HasColumnName("shuntBEnabled");

            entity.Property(e => e.ShuntCamps)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("shuntCAmps")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.ShuntCenabled).HasColumnName("shuntCEnabled");
        });

        modelBuilder.Entity<OutbackMateInverterChargerRecord>(entity =>
        {
            entity.HasNoKey();

            entity.ToView("OutbackMateInverterChargerRecords_");

            entity.Property(e => e.AcInputMode).HasColumnName("acInputMode");

            entity.Property(e => e.AcInputVoltage).HasColumnName("acInputVoltage");

            entity.Property(e => e.AcOutputVoltage).HasColumnName("acOutputVoltage");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BuyCurrent).HasColumnName("buyCurrent");

            entity.Property(e => e.ChargerCurrent).HasColumnName("chargerCurrent");

            entity.Property(e => e.ErrorMode).HasColumnName("errorMode");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.InverterCurrent).HasColumnName("inverterCurrent");

            entity.Property(e => e.Misc).HasColumnName("misc");

            entity.Property(e => e.OperationalMode).HasColumnName("operationalMode");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SellCurrent).HasColumnName("sellCurrent");

            entity.Property(e => e.WarningMode).HasColumnName("warningMode");
        });

        modelBuilder.Entity<OutbackMateInverterChargerRecordNew>(entity =>
        {
            entity.ToTable("OutbackMateInverterChargerRecords_NEW");

            entity.HasIndex(x => new { x.HubPort, x.InverterCurrent, x.ChargerCurrent, x.AcInputVoltage, x.AcOutputVoltage, x.BatteryVoltage, x.ErrorMode, x.OperationalMode, x.WarningMode, x.RecordDateTime }/*, "IX_OutbackMateInverterChargerRecords_cover01"*/);

            entity.HasKey(x => x.Id)
                .HasName("PK_OutbackMateInverterChargerRecord")
                .IsClustered(false);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.AcInputMode).HasColumnName("acInputMode");

            entity.Property(e => e.AcInputVoltage).HasColumnName("acInputVoltage");

            entity.Property(e => e.AcOutputVoltage).HasColumnName("acOutputVoltage");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BuyCurrent).HasColumnName("buyCurrent");

            entity.Property(e => e.ChargerCurrent).HasColumnName("chargerCurrent");

            entity.Property(e => e.ErrorMode).HasColumnName("errorMode");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.InverterCurrent).HasColumnName("inverterCurrent");

            entity.Property(e => e.Misc).HasColumnName("misc");

            entity.Property(e => e.OperationalMode).HasColumnName("operationalMode");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SellCurrent).HasColumnName("sellCurrent");

            entity.Property(e => e.WarningMode).HasColumnName("warningMode");
        });

        modelBuilder.Entity<OutbackMateInverterChargerRecordsOneMinuteArchive>(entity =>
        {
            entity.ToTable("OutbackMateInverterChargerRecords_OneMinuteArchive");

            entity.HasIndex(x => x.RecordDateTime/*, "IX_OutbackMateInverterChargerRecords_OneMinuteArchive_RecordDateTime"*/);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.AcInputMode).HasColumnName("acInputMode");

            entity.Property(e => e.AcInputVoltage).HasColumnName("acInputVoltage");

            entity.Property(e => e.AcOutputVoltage).HasColumnName("acOutputVoltage");

            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(9, 2)")
                .HasColumnName("batteryVoltage")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.BuyCurrent).HasColumnName("buyCurrent");

            entity.Property(e => e.ChargerCurrent).HasColumnName("chargerCurrent");

            entity.Property(e => e.ErrorMode).HasColumnName("errorMode");

            entity.Property(e => e.HubPort).HasColumnName("hubPort");

            entity.Property(e => e.InverterCurrent).HasColumnName("inverterCurrent");

            entity.Property(e => e.Misc).HasColumnName("misc");

            entity.Property(e => e.OperationalMode).HasColumnName("operationalMode");

            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.SellCurrent).HasColumnName("sellCurrent");

            entity.Property(e => e.WarningMode).HasColumnName("warningMode");
        });

        modelBuilder.Entity<SecurityCameraRecord>(entity =>
        {
            entity.HasIndex(x => new { x.RecordDateTime, x.CameraNumber, x.ImageType }/*, "IX_SecurityCameraRecords"*/);

            entity.Property(e => e.StorageLocation)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<CameraRecord>(entity =>
        {
            entity.HasIndex(x => new { x.RecordDateTime, x.CameraNumber, x.ImageType, x.CameraType }/*, "IX_CameraRecords"*/);

            entity.Property(e => e.StorageLocation)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<SkyMonitor>(entity =>
        {
            entity.ToTable("SkyMonitor");

            entity.HasIndex(x => x.RecordDateTime/*, "IX_SkyMonitor_RecordDateTime"*/);

            entity.Property(e => e.AmbientTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.Gain)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Ir)
                .HasColumnType("decimal(9, 4)")
                .HasColumnName("IR")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 4)");

            entity.Property(e => e.Lux)
                .HasColumnType("decimal(9, 4)")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 4)");

            entity.Property(e => e.SkyTemperature)
                .HasColumnType("decimal(9, 2)")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 2)");

            entity.Property(e => e.Visible)
                .HasColumnType("decimal(9, 4)")
                .HasAnnotation("Relational:ColumnType", "decimal(9, 4)");
        });

        modelBuilder.Entity<WeatherCameraRecord>(entity =>
        {
            entity.HasIndex(x => new { x.RecordDateTime, x.CameraNumber, x.ImageType }/*, "IX_WeatherCameraRecords"*/);

            entity.Property(e => e.StorageLocation)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<WeatherSatelliteRecord>(entity =>
        {
            entity.HasIndex(x => new { x.RecordDateTime, x.ImageType }/*, "IX_WeatherSatelliteRecords"*/);

            entity.Property(e => e.StorageLocation)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<WebPowerSwitchConfiguration>(entity =>
        {
            entity.HasIndex(x => x.SerialNumber/*, "IX_WebPowerSwitchConfiguration_SerialNumber"*/)
                .IsUnique();

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.Property(e => e.SerialNumber)
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RawModels.WeatherRecordHighLowSummary>(entity =>
        {
            entity.HasKey(e => new { e.StartRecordDateTime, e.EndRecordDateTime });
            entity.Property(e => e.BarometerHigh).HasColumnName("barometerHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.BarometerLow).HasColumnName("barometerLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.InsideTemperatureHigh).HasColumnName("insideTemperatureHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.InsideTemperatureLow).HasColumnName("insideTemperatureLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.BarometerHigh).HasColumnName("barometerHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.BarometerLow).HasColumnName("barometerLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.OutsideDewpointHigh).HasColumnName("outsideDewpointHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideDewpointLow).HasColumnName("outsideDewpointLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.OutsideHeatIndexHigh).HasColumnName("outsideHeatIndexHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideHeatIndexLow).HasColumnName("outsideHeatIndexLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.OutsideTemperatureHigh).HasColumnName("outsideTemperatureHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideTemperatureLow).HasColumnName("outsideTemperatureLow").HasColumnType("decimal(9,2)");

            entity.Property(e => e.OutsideWindChillHigh).HasColumnName("outsideWindChillHigh").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideWindChillLow).HasColumnName("outsideWindChillLow").HasColumnType("decimal(9,2)");
        });

        modelBuilder.Entity<RawModels.DavisVantageProAverage>(entity =>
        {
            entity.HasKey(e => e.RecordDateTime);
            entity.Property(e => e.RecordDateTime).HasColumnName("recordDateTime");

            entity.Property(e => e.Barometer).HasColumnName("barometer").HasColumnType("decimal(9,2)");
            entity.Property(e => e.InsideHumidity).HasColumnName("insideHumidity");
            entity.Property(e => e.InsideTemperature).HasColumnName("insideTemperature").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideDewpoint).HasColumnName("outsideDewpoint").HasColumnType("decimal(9,2)");
            entity.Property(e => e.OutsideHumidity).HasColumnName("outsideHumidity");
            entity.Property(e => e.OutsideTemperature).HasColumnName("outsideTemperature").HasColumnType("decimal(9,2)");
            entity.Property(e => e.SolarRadiation).HasColumnName("solarRadiation");
            entity.Property(e => e.WindDirection).HasColumnName("windDirection");
            entity.Property(e => e.WindSpeed).HasColumnName("windSpeed");
            entity.Property(e => e.WindSpeedHigh).HasColumnName("windSpeedHigh");
            entity.Property(e => e.WindSpeedLow).HasColumnName("windSpeedLow");
        });


        OnModelCreatingPartial(modelBuilder);
    }


    public virtual async Task<RawModels.WeatherRecordHighLowSummary> GetWeatherRecordHighLowSummary(DateTimeOffset startRecordDateTime, DateTimeOffset endRecordDateTime)
    {
        var p = new Microsoft.Data.SqlClient.SqlParameter[]
        {
                new Microsoft.Data.SqlClient.SqlParameter("@iStartRecordDateTime", System.Data.SqlDbType.DateTimeOffset) { Value = startRecordDateTime },
                new Microsoft.Data.SqlClient.SqlParameter("@iEndRecordDateTime", System.Data.SqlDbType.DateTimeOffset) { Value = endRecordDateTime }
        };

        var r = await this.WeatherRecordHighLowSummary.FromSqlRaw("sp__GetWeatherRecordHighLowSummary @iStartRecordDateTime, @iEndRecordDateTime", p)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        return r.FirstOrDefault();
    }

    public virtual async Task<IEnumerable<RawModels.DavisVantageProAverage>> GetDavisVantageProOneMinuteAverage(DateTimeOffset startRecordDateTime, DateTimeOffset endRecordDateTime)
    {
        var p = new Microsoft.Data.SqlClient.SqlParameter[]
        {
                new Microsoft.Data.SqlClient.SqlParameter("@iRecordDateTimeStart", System.Data.SqlDbType.DateTimeOffset) { Value = startRecordDateTime },
                new Microsoft.Data.SqlClient.SqlParameter("@iRecordDateTimeEnd", System.Data.SqlDbType.DateTimeOffset) { Value = endRecordDateTime }
        };

        return await this.DavisVantageProAverage.FromSqlRaw("ef__GetDavisVantageProOneMinuteAverage @iRecordDateTimeStart, @iRecordDateTimeEnd", p)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }


    public virtual async Task<IEnumerable<CameraRecord>> GetLatestCameraRecordsAsync(byte cameraType, byte imageType)
    {
        var p = new Microsoft.Data.SqlClient.SqlParameter[]
        {
                new Microsoft.Data.SqlClient.SqlParameter("@iCameraType", System.Data.SqlDbType.TinyInt) { Value = cameraType },
                new Microsoft.Data.SqlClient.SqlParameter("@iImageType", System.Data.SqlDbType.TinyInt) { Value = imageType }
        };

        return await this.CameraRecords.FromSqlRaw("ef__v7__GetLatestCameraRecords @iCameraType, @iImageType", p)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
