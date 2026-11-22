using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.Modules;

/// <summary>
/// The Engine that enriches numerical meterological models and 
/// translates the Values into a practical daily forecast for anyone to use.
/// 
/// Data points needed:

/// example. Good Day. On thursday the 17th (note me: come up with an overall score for the day) the forecast looks (Something to do with
/// the rating) spectacular.(Unless something that trigger's a weather record happens in the early morning, start at morning.) In the morning 
/// </summary>
public partial class ForecastService : ObservableObject, IForecastService
{
    [ObservableProperty]
    public partial int TCount { get; set; }
    private ISession _session;

    [ObservableProperty]
    public partial NwsHourlyForecast[] NwsHourlies { get; set; }
    [ObservableProperty]
    public partial HourlyForecast[] gfsHourlies { get; set; }
    [ObservableProperty]
    public partial DateTime SelectedTime { get; set; }
    [ObservableProperty]
    public partial CalendarDayModel[] Dates { get; set; }
    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<ForecastInputs> Inputs { get; set; } = new ObservableCollection<ForecastInputs>();
    [ObservableProperty]
    public partial ObservableCollection<TimeOfDayGroup> TimeOfDayDailies { get; set; } = new ObservableCollection<TimeOfDayGroup>();
    [ObservableProperty]
    public partial ObservableCollection<NwsTimeOfDayForecast> TimeOfDayForecasts { get; set; } = new ObservableCollection<NwsTimeOfDayForecast>();
    //public static string Temperature { get; set; }
    //public static string HealthSafetyStatement { get; set; }
    //public static string Ptsi { get; set; }
    //public static string SkyCover { get; set; }
    /////     - Ceiling Height
    //public static string Winds { get; set; }
    //public static string GustsStatement { get; set; }
    //public static string PrecipitationStatement { get; set; }
    //public static string StormsStatement { get; set; }
    public ForecastService(ISession session)
    {

        _session = session;

    }
    // Temp
    // Time Of Day Forecast Sample:
    // The {TimeOfDay } of {DDDD MMMM dd} (StartTime} - {EndTime}):
    // The morning of November 2nd (6am - 12pm):
    // Temperature {temp} and {trend}.
    // Temperature in the mid 50's and climbing.
    //{If ShortForecast Exists}
    // LOOKOUT! There is weather afoot...
    //{precipitationCategory} is moving into {city} by {StormStartTime}.
    // rain is moving into Boston by 8am.
    //{precipationType} will start off {intensity} and
    // The rain will start off light and
    // {trend} to {intensity} by {StormEndTime}.
    // increase to heavy rain showers by 10am. 
    // {if quantitativePrecipitation has values of zero inbetween the start and end}
    // The rain will be on and off
    // throughout the morning and
    //totaling about {Sum quantitativePrecipitation}.
    // total about a quarter of an inch.
    //{skyCover description and if skyCover description changes significantly}
    // We won't see the sun break through
    // the overcast sky until
    // later in the day.
    // Dominant winds from the
    // {winddirection} prevail from {minSpeed} up to {maxSpeed} mph
    // northeast prevail at speeds up to 25 mph
    // {gust statement}
    // , and gusts up to 35 mph.
    // --- Time Of Day Creating 
    // The hourlies being processed are being processed through a for loop in SelectLocationViewModel Line: 
    public async Task<NwsTimeOfDayForecast[]> BuildTimeOfDayForecastsAsync(NwsHourlyForecast[] nws)
    {
        try
        {
            HoursGroups hg;
            var dayGroup = nws.OrderBy(h => h.StartTime).GroupBy(g => g.StartTime.Date);
            foreach (var day in dayGroup)
            {
                var date = day.Key;
                var hours = day.ToArray();
                var tod = hours.GroupBy(x => x.TimeOfDay);
                foreach (var time in tod)
                {
                    string timeOfDay = time.Key;
                    var todHours = time.ToArray();
                    var timeOfDayForecast = new NwsTimeOfDayForecast();
                    timeOfDayForecast.StartTime = todHours.Min(x => x.StartTime);
                    timeOfDayForecast.EndTime = todHours.Max(x => x.EndTime);

                    Debug.WriteLine($"There are {todHours.Length}");

                    var times = timeOfDayForecast.StartTime.Hour switch
                    {
                        >= 0 and < 6 => ("Early Morning", "PeriodEarlyMorning", "time_morning.json", false),
                        >= 6 and < 12 => ("Morning", "PeriodMorning", "time_late_morning.json", true),
                        >= 12 and < 18 => ("Afternoon", "PeriodAfternoon", "time_afternoon.json", true),
                        >= 18 and < 22 => ("Evening", "PeriodEvening", "time_evening.json", false),
                        _ => ("Night", "PeriodNight", "time_night.json", false)
                    };

                    timeOfDayForecast.TimeOfDay = times.Item1;
                    timeOfDayForecast.IsDay = times.Item4;

                    timeOfDayForecast.TimeOfDayGradientBrush = (LinearGradientBrush)Application.Current.Resources[times.Item2];
                    var animation = new SKFileLottieImageSource { File = times.Item3 };
                    var weatherAnimations = hours.Select(x => new LottieAnimation
                    {
                        Temperature = x.TempAndUnit,
                        Time = x.StartTime,
                        Forecast = x.ShortForecast,
                        LottieWeather = x.SkHourlyAnimation
                    }).ToObservableCollection<LottieAnimation>();
                    timeOfDayForecast.SkLottieCollection = weatherAnimations;
                    timeOfDayForecast.SKTimeOfDayAnimation = animation;
                    timeOfDayForecast.Forecasts = todHours.ToArray();
                    timeOfDayForecast.Events = _session.EventTimes.Where(x => x.WeatherHour >= timeOfDayForecast.StartTime && timeOfDayForecast.EndTime >= x.WeatherHour).ToObservableCollection();
                    var results = GetDailyHighLow(timeOfDayForecast.StartTime.Date);
                    timeOfDayForecast.High = results.high;
                    timeOfDayForecast.Low = results.low;
                    await BuildForecastAsync(timeOfDayForecast);
                    ++TCount;
                    if (TimeOfDayForecasts.Count == TCount)
                    {
                        Debug.WriteLine($"{timeOfDayForecast.TimeOfDay} has been added for {timeOfDayForecast.StartTime.Date:MM/dd}.");
                    }
                    else
                    {
                        Debug.WriteLine($"{timeOfDayForecast.TimeOfDay} was not successfully added for {timeOfDayForecast.StartTime.Date:MM/dd}.");
                    }
                }
                Debug.WriteLine($"{TimeOfDayForecasts.Last().TimeOfDay} was added.");
            }
            Debug.WriteLine($"{TimeOfDayForecasts.Last().EndTime.ToString("F")} was added.");

            var timeOfDayArray = TimeOfDayForecasts.OrderBy(s => s.StartTime).ToArray();
            return timeOfDayArray;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR in Forecast Service. Unable to create the Times of Days for each day. {ex}");
            return default;
        }
    }

    private (string high, string low) GetDailyHighLow(DateTime date)
    {
        var highRecord = _session.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == date.Date && x.Temperature.Value != null).MaxBy(y => y.Temperature.Value);
        var lowRecord = _session.NwsData.HourlyForecasts.Where(x => x.StartTime.Date == date.Date && x.Temperature.Value != null).MinBy(z => z.Temperature.Value);
        string high = string.Empty;
        string low = string.Empty;
        if (highRecord == null)
        {
            high = $"The high temperature{Environment.NewLine}is not available.";
        }
        else
        {
            high = $"High of {highRecord.TempAndUnit}{Environment.NewLine}@ {highRecord.StartTime:h tt}.";
        }

        if (lowRecord == null)
        {
            low = $"The Low temperature{Environment.NewLine}is not available.";
        }
        else
        {
            low = $"Low of {lowRecord.TempAndUnit}{Environment.NewLine}@ {lowRecord.StartTime:h tt}.";
        }

        return (high, low);
    }


    public async Task BuildForecastAsync(NwsTimeOfDayForecast timeOfDayForecast)
    {
        //Tabular Forecast

        var openSb = new StringBuilder();
        var forecastSb = new StringBuilder();
        var weatherSb = new StringBuilder();
        var precipitationSb = new StringBuilder();
        var tempSb = new StringBuilder();
        var skySb = new StringBuilder();
        var windSb = new StringBuilder();
        var inputs = new ObservableCollection<ForecastInputs>();
        try
        {
            var nws = timeOfDayForecast.Forecasts.ToArray();
            inputs = BuildForecastInputs(nws);
            timeOfDayForecast.DewPointAvg = inputs.Average(x => x.Dewpoint.Value);
            timeOfDayForecast.MaxProbabilityOfPrecipitation = inputs.Max(x => x.Precipitation.Value);
            timeOfDayForecast.MaxTemp = inputs.Max(x => x.Temp.Value);
            timeOfDayForecast.MaxWindGusts = inputs.Where(s => s.WindGusts != null).Max(x => x.WindGusts.Value);
            timeOfDayForecast.MaxWindSpeed = inputs.Max(x => x.Wind.Value);
            timeOfDayForecast.MaxWindDirection = inputs.MaxBy(x => x.Wind.Value).Wind.Direction;
            timeOfDayForecast.MinTemp = inputs.Min(x => x.Temp.Value);
            timeOfDayForecast.MinWindGusts = inputs.Where(s => s.WindGusts != null).Min(x => x.WindGusts.Value);
            timeOfDayForecast.MinWindSpeed = inputs.Min(x => x.Wind.Value);
            timeOfDayForecast.MinWindDirection = inputs.MinBy(x => x.Wind.Value).Wind.Direction;
            timeOfDayForecast.QuantitativePrecipitation = inputs.Sum(x => x.Precipitation.Amount);
            timeOfDayForecast.RelativeHumidityAvg = inputs.Average(x => x.RelativeHumidity.Value);
            var forecasts = inputs.Select(x => x.WmoForecast.ToLowerInvariant()).Distinct();
            openSb.AppendLine($@"{nws.First().StartTime.ToString("MMMM d")} ({nws.First().StartTime.ToString("h tt")} - {nws.Last().EndTime.ToString("h tt")}):");
            timeOfDayForecast.DescForecasts = forecasts.Count() == 1 ? $"{forecasts.First()}" : $"{String.Join(", then ", forecasts.SkipLast(1))} and then {forecasts.LastOrDefault()}.";

            Debug.WriteLine($"{timeOfDayForecast.TimeOfDayName} have been processed.");

            var forecastGroup = inputs.GroupBy(x => x.WmoForecast);
            forecastSb.AppendLine("Forecast:");
            foreach (var item in forecastGroup)
            {
                if (item.Key == null) continue;
                var sTime = item.Min(x => x.StartTime);
                var eTime = item.Max(x => x.EndTime);
                forecastSb.AppendLine($"From {sTime.ToString("h tt")} to {eTime.ToString("h tt")} {item.Key}.");
            }
            var temps = inputs.GroupBy(x => x.TempStatement);
            tempSb.AppendLine("Temperature:");
            foreach (var tempChange in temps)
            {
                tempSb.AppendLine($"From {tempChange.Min(x => x.Temp.StartTime).ToString("h tt")} to {tempChange.Max(x => x.Temp.EndTime).ToString("h tt")} {tempChange.Key}.");
            }
            var weatherGrouped = inputs.GroupBy(x => x.StormWeatherStatement);

            foreach (var storm in weatherGrouped)
            {
                if (storm.Key == null) continue;
                weatherSb.AppendLine("Storm Weather:");
                weatherSb.AppendLine($"From {storm.Min(x => x.StartTime.ToString("h tt"))} to {storm.Max(x => x.EndTime.ToString("h tt"))} {storm.Key}");
            }
            var windGrouped = inputs.GroupBy(x => x.WindGustStatement);
            windSb.AppendLine("Winds and Gusts:");
            foreach (var wind in windGrouped)
            {
                windSb.AppendLine($"From {wind.Min(x => x.StartTime.ToString("h tt"))} to {wind.Max(x => x.EndTime.ToString("h tt"))} {wind.Key}");
            }
            var precipitationGrouped = inputs.GroupBy(x => x.Precipitation.Type);
            foreach (var type in precipitationGrouped)
            {
                if (type.Key == null) continue;
                precipitationSb.AppendLine($"Precipitation({type.Key}):");
                precipitationSb.AppendLine($"Total: {type.Sum(x => x.Precipitation.Amount)} for {type.Max(x => x.StartTime.ToString("dddd, MMM dd"))}");
            }
            var skyGrouped = inputs.GroupBy(x => x.SkyCoverStatement);
            skySb.AppendLine("Sky Cover:");
            foreach (var sky in skyGrouped)
            {
                skySb.AppendLine($"From {sky.Min(x => x.StartTime.ToString("h tt"))} to {sky.Max(x => x.EndTime.ToString("h tt"))} {sky.Key}");
            }
            string finalSb = openSb.ToString() + forecastSb.ToString() + weatherSb.ToString() + precipitationSb.ToString() + tempSb.ToString() + skySb.ToString() + windSb.ToString();
            timeOfDayForecast.LongForecast = finalSb;
            TimeOfDayForecasts.Add(timeOfDayForecast);
            return;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR running BuildForecastAsync method in Forecast Service {ex}");
            return;
        }

    }


    private string CoverageStmt(string coverage, string weather, string intensity = "")
    {
        string weatherDescription = string.Empty;
        weatherDescription += coverage switch
        {
            "areas" => "areas of ",
            "brief" => "brief ",
            "chance" => "chance of ",
            "definite" => "definite ",
            "few" => "few ",
            "frequent" => "frequent ",
            "intermittent" => "intermittent ",
            "isolated" => "isolated ",
            "likely" => "likely chance of ",
            "numerous" => "numerous ",
            "occasional" => "occasional ",
            "patchy" => "patchy ",
            "periods" => "periods of ",
            "scattered" => "scattered ",
            "slight_chance" => "slight chance of ",
            "widespread" => "widespread ",
            null => "",
            "" => "",
            _ => "",
        };
        weatherDescription += " " + intensity.Replace("_", " ").ToLowerInvariant() ?? ""; weatherDescription += " " + weather.Replace("_", " ").ToLowerInvariant();
        return weatherDescription;
    }

    public ObservableCollection<ForecastInputs> BuildForecastInputs(NwsHourlyForecast[] hours)
    {
        var inputCollection = new ObservableCollection<ForecastInputs>();
        if (hours == null || hours.Length == 0)
            return inputCollection;

        var weather = new ObservableCollection<WeatherStatementsValue>();
        var stormBuilder = new StringBuilder();


        for (int h = 0; h < hours.Length; h++)
        {
            weather.Clear();
            stormBuilder.Clear();

            var supp = hours[h].Supplemental ?? new NwsHourlySupplemental();

            if (supp.Weather != null && supp.Weather.Length > 0)
            {
                for (int w = 0; w < supp.Weather.Length; w++)
                {
                    if (supp.Weather[w] != null)
                        weather.Add(supp.Weather[w]!);
                }

                for (int w = 0; w < weather.Count; w++)
                {

                    if (w == 0)
                    {
                        stormBuilder.Append($"Expect {CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity ?? "")}");
                        if (weather.Count == 1 && !weather.Any(x => x.Weather.ToLowerInvariant().Contains("thunder")) && (supp.ProbabilityOfThunder?.Value ?? 0) > 0)
                        {
                            stormBuilder.Append($"and {supp.ProbabilityOfThunder?.Description}");
                        }
                    }
                    if (w == 1)
                    {
                        stormBuilder.Append($"along with {CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity ?? "")}");
                        if (weather.Count > 1 && !weather.Any(x => x.Weather.ToLowerInvariant().Contains("thunder")) && (supp.ProbabilityOfThunder?.Value ?? 0) > 0)
                        {
                            stormBuilder.Append($"and {supp.ProbabilityOfThunder?.Description}");
                        }
                    }
                    if (w > 1)
                    {
                        stormBuilder.Append($",{CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity ?? "")}");
                    }
                    if (weather.Count - 1 == w)
                    {
                        stormBuilder.Append($" lasting about {weather[w].Duration} hours.");
                    }
                }
            }

            var forecast = new ForecastInputs
            {
                StartTime = hours[h].StartTime,
                EndTime = hours[h].EndTime,
                WmoForecast = hours[h].ShortForecast,
                //Temperature
                Temp = supp.Temperature != null ? new ForecastTemperature
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.Temperature.Value,
                    UnitCode = supp.Temperature.UnitCode
                } : new ForecastTemperature(),
                Dewpoint = supp.DewPoint != null ? new ForecastDewPoint
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    UnitCode = supp.DewPoint.UnitCode,
                    Value = supp.DewPoint.Value
                } : new ForecastDewPoint(),
                RelativeHumidity = supp.RelativeHumidity != null ? new ForecastRelativeHumidity
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    UnitCode = supp.RelativeHumidity.UnitCode,
                    Value = supp.RelativeHumidity.Value
                } : new ForecastRelativeHumidity(),
                ApparentTemperature = supp.ApparentTemperature != null ? new ForecastApparentTemperature
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.ApparentTemperature.Value,
                    UnitCode = supp.ApparentTemperature.UnitCode
                } : new ForecastApparentTemperature(),
                WindChill = supp.WindChill != null ? new ForecastWindChill
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.WindChill.Value,
                    UnitCode = supp.WindChill.UnitCode
                } : new ForecastWindChill(),
                HeatIndex = supp.HeatIndex != null ? new ForecastHeatIndex
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.HeatIndex.Value,
                    UnitCode = supp.HeatIndex.UnitCode
                } : new ForecastHeatIndex(),
                HeatRisk = supp.HeatRisk != null ? new ForecastHeatRisk
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.HeatRisk.Value,
                    UnitCode = supp.HeatRisk.UnitCode
                } : new ForecastHeatRisk(),
                WetBulbGlobeTemp = supp.WetBulbGlobeTemperature != null ? new ForecastWbgt
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.WetBulbGlobeTemperature.Value,
                    UnitCode = supp.WetBulbGlobeTemperature.UnitCode
                } : new ForecastWbgt(),
                //Wind
                Wind = supp.WindSpeed != null ? new ForecastWind
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.WindSpeed.Value,
                    Direction = supp.WindDirection?.Description ?? hours[h].WindDirection ?? string.Empty,
                    UnitCode = hours[h].WindSpeed?.UnitCode ?? supp.WindSpeed.UnitCode
                } : new ForecastWind(),
                WindGusts = supp.WindGust != null ? new ForecastGusts
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.WindGust.Value,
                    UnitCode = supp.WindGust.UnitCode
                } : new ForecastGusts(),
                TransportWind = supp.TransportWindSpeed != null ? new ForecastTransportWind
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.TransportWindSpeed.Value,
                    Direction = supp.TransportWindDirection != null ? $@"({supp.TransportWindDirection.Value}) {supp.TransportWindDirection.Direction}" : string.Empty,
                    UnitCode = supp.TransportWindSpeed.UnitCode
                } : new ForecastTransportWind(),
                //Precipitation
                Weather = new ObservableCollection<WeatherStatementsValue>(weather),
                StormWeatherStatement = stormBuilder.ToString(),
                Precipitation = supp.QuantitativePrecipitation != null && hours[h].ProbabilityOfPrecipitation != null ? new ForecastPrecipitation
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = hours[h].ProbabilityOfPrecipitation.Value,
                    Amount = supp.QuantitativePrecipitation.Value,
                    UnitCode = supp.QuantitativePrecipitation.UnitCode
                } : new ForecastPrecipitation(),
                IceAccumulation = supp.IceAccumulation != null ? new ForecastIceAccumulation
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.IceAccumulation.Value,
                    UnitCode = supp.IceAccumulation.UnitCode
                } : new ForecastIceAccumulation(),
                SnowFallAmount = supp.SnowfallAmount != null ? new ForecastSnowfallAmount
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.SnowfallAmount.Value,
                    UnitCode = supp.SnowfallAmount.UnitCode
                } : new ForecastSnowfallAmount(),
                Thunder = supp.ProbabilityOfThunder != null ? new ForecastThunder
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.ProbabilityOfThunder.Value,
                    UnitCode = supp.ProbabilityOfThunder.Description
                } : new ForecastThunder(),
                //Clouds
                SkyCover = supp.Skycover != null ? new ForecastSkyCover
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.Skycover.Value,
                    UnitCode = supp.Skycover.UnitCode
                } : new ForecastSkyCover(),
                Lovri = supp.LowVisibilityOccurrenceRiskIndex != null ? new ForecastLowOccuranceVisibilityIndex
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.LowVisibilityOccurrenceRiskIndex.Value,
                    UnitCode = supp.LowVisibilityOccurrenceRiskIndex.UnitCode
                } : new ForecastLowOccuranceVisibilityIndex(),
                Visibility = supp.Visibility != null ? new ForecastVisibility
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.Visibility.Value,
                    UnitCode = supp.Visibility.UnitCode
                } : new ForecastVisibility(),
                CeilingHeight = supp.CeilingHeight != null ? new ForecastCeilingHeight
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.CeilingHeight.Value,
                    UnitCode = supp.CeilingHeight.UnitCode
                } : new ForecastCeilingHeight(),
                MixingHeight = supp.MixingHeight != null ? new ForecastMixingHeight
                {
                    StartTime = hours[h].StartTime,
                    EndTime = hours[h].EndTime,
                    Value = supp.MixingHeight.Value,
                    UnitCode = supp.MixingHeight.UnitCode
                } : new ForecastMixingHeight(),
                TempStatement = TempCategory(
                    hours[h].Temperature?.Value ?? 0,
                    hours[h].DewPoint?.Value ?? 0,
                    hours[h].RelativeHumidity?.Value ?? 0,
                    supp.WetBulbGlobeTemperature?.Value ?? 0),
                WindGustStatement = $"Winds from the {hours[h].WindDirection} at {hours[h].WindSpeed?.Value ?? 0}{hours[h].WindSpeed?.UnitCode}{(hours[h].WindGust != null ? $"{hours[h].WindGust.Value} mph." : ".")}",
                SkyCoverStatement = (supp.Skycover?.Value ?? 0) switch
                {
                    < 10 => $"Look up to the sky and see a clear{(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? " sunny day" : " stary night")}.",
                    >= 10 and < 30 => $"Look up to the sky and see a mostly {(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? "sunny day" : "stary night")}.",
                    >= 30 and < 60 => $"Look up to the sky and see a partly {(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? "sunny day" : "stary night")}.",
                    >= 60 and < 70 => $"Look up to the sky and see a partly cloudy {(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? "day" : "night")}.",
                    >= 70 and < 90 => $"Look up to the sky and see a mostly cloudy {(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? "day" : " night")}.",
                    >= 90 => $"Look up to the sky and see an overcast {(hours[h].StartTime.Hour >= 6 || hours[h].StartTime.Hour < 18 ? "day" : " night")}."
                }
            };

            inputCollection.Add(forecast);
        }
        return inputCollection;
    }

    public string TempCategory(double temp, double dewPoint, double relativeHumidity, double wbgt)
    {
        var sb = new StringBuilder();
        sb.Append($"The temperature will be {NumberGroups.TempsToString(temp)} with a dew point {NumberGroups.DewPointToString(dewPoint)}. Expect ");
        // --- Extreme heat danger (WBGT-based) ---
        if (wbgt >= 90)
        {
            sb.Append("extreme dangerous heat this hour.");
            return sb.ToString();
        }
        if (wbgt >= 82)
        {
            sb.Append("dangerous heat this hour.");
            return sb.ToString();
        }
        if (wbgt >= 78)
        {
            sb.Append("very hot and humid this hour.");
            return sb.ToString();
        }
        // --- RelativeHumidity / dew point discomfort ---
        bool oppressiveHumidity = dewPoint >= 75 || relativeHumidity >= 85;
        bool veryHumid = dewPoint >= 70 || relativeHumidity >= 75;
        bool humid = dewPoint >= 65 || relativeHumidity >= 60;

        // --- Cold stress ---
        if (temp <= 20)
        {
            sb.Append("frigid conditions this hour.");
            return sb.ToString();
        }
        if (temp <= 32)
        {
            sb.Append("freezing conditions this hour.");
            return sb.ToString();
        }
        if (temp <= 40)
        {
            sb.Append(humid ? "raw and cold conditions this hour." : "cold conditions this hour.");
            return sb.ToString();
        }
        // --- Cool to mild ---
        if (temp <= 50)
        {
            sb.Append(humid ? "chilly and damp conditions this hour." : "cool conditions this hour.");
            return sb.ToString();
        }
        if (temp <= 60)
        {
            sb.Append(humid ? "cool and humid conditions this hour." : "mildly cool conditions this hour.");
            return sb.ToString();
        }
        // --- Mild to warm ---
        if (temp <= 70)
        {
            sb.Append(humid ? "mild but humid conditions this hour." : "mild conditions this hour.");
            return sb.ToString();
        }
        if (temp <= 80)
        {
            if (oppressiveHumidity) sb.Append("warm and oppressive conditions this hour.");
            if (veryHumid) sb.Append("warm and very humid conditions this hour.");
            if (humid) sb.Append("warm and humid conditions this hour.");
            sb.Append("warm conditions this hour.");
            return sb.ToString();
        }

        // --- Hot categories ---
        if (temp <= 90)
        {
            if (oppressiveHumidity) sb.Append("hot and oppressive conditions this hour.");
            if (veryHumid) sb.Append("hot and very humid conditions this hour.");
            if (humid) sb.Append("hot and humid conditions this hour.");
            sb.Append("hot conditions this hour.");
            return sb.ToString();
        }

        // --- Extreme heat (temp-based) ---
        if (temp > 90)
        {
            if (oppressiveHumidity) sb.Append("extremely hot and oppressive conditions this hour.");
            if (veryHumid) sb.Append("extremely hot and very humid conditions this hour.");
            sb.Append("extremely hot conditions this hour.");
            return sb.ToString();
        }

        sb.Append("unknown");
        return sb.ToString();
    }

    // Produces the enriched string of forecasts (await)


    // Start Intensity and start time
    //Any changes in intensity if every hour is an up down change
    // on and off or switch between lighter and heavier
    //Any changes in coverage should also be shown
    // qprecip shows if it's raining heavier one hour over another hour
    //skycover increases and then decreases 
    // total length of storm and total precipitation that falls
    // use for loops to see if there are any pockets in the weather or times of peace.
    // those are the on and off again's.
    // maybe even talk about the wind speeds during the weather including gust statements.
    // don't forget Thunderstorms are storms too that don't always show up with weather.
    // HeatRisk index if high enough can also be weather as well as (find out about cold exposure and hypothermia).
    // Future releases will also have the Nws Alerts, alerting of flash floods, winter storm advisories, winter storm warnings, heat advisories, drought conditions.
    //During intense storms, check the distance of the sun and moon and see if they are contributing to exceptionally higher tides
    // For peoples who like to go outside the climate control, new healf and safety with web bulb gt and apparenttemp, dewpoint, relative humidity
    //    // 


    //foreach (var timeOfDay in grouped)
    //{
    //    var todForecast = timeOfDay.ToObservableCollection();

    //    var result = TemperatureEngine.StandardDeviation(todForecast);
    //}



    //for (int i = 0; i < inputs.Count - 1; i++)
    //{
    //    if (Math.Round(inputs[i].Temp / 3) * 3 > Math.Round(inputs[i + 1].Temp / 3) * 3)
    //        inputs[i].TempTrend = "cooling down";
    //    if (Math.Round(inputs[i].Temp / 3) * 3 < Math.Round(inputs[i + 1].Temp / 3) * 3)
    //        inputs[i].TempTrend = "warming up";
    //    if (Math.Round(inputs[i].Temp / 3) * 3 == Math.Round(inputs[i + 1].Temp / 3) * 3)
    //        inputs[i].TempTrend = "remaining steady";
    //}
    //var startTime = inputs.Min(x => x.StartTime);
    //var endTime = inputs.Max(x => x.EndTime);
    // The morning of November 2 (6 AM - 12 PM)

    //Temps and times
    //var maxTemp = inputs.MaxBy(x => x.Temp);
    //var minTemp = inputs.MinBy(x => x.Temp);


    //starterCollection.Add((starttime, endtime, line));
    //tempCollection.Add((starttime, endtime, ProcessTrend(temp, futureTemp, starttime, endtime), temp));
    //windCollection.Add((starttime, endtime, ProcessTrend(windSpeed, futureWindSpeed, starttime, endtime), windSpeed, direction));
    //gustsCollection.Add((starttime, endtime, ProcessTrend(gust, futureGust, starttime, endtime), gust));
    //skyCoverCollection.Add((starttime, endtime, ProcessTrend(skyCover, futureSkyCover, starttime, endtime), skyCover));
    //tStormCollection.Add((starttime, endtime, ProcessTrend(tStorm, futureTStorm, starttime, endtime), tStorm));


    //Starter
    //var orderStarters = starterCollection.OrderBy(x => x.starttime).GroupBy(y => y.line);
    //var phrases = orderStarters.Select(x => $"From {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")}, {x.Key.ToLowerInvariant()}").ToObservableCollection();
    //var firstLine = $"{String.Join(", ", phrases.Take(phrases.Count - 1))} and {phrases.Last()}.";
    ////Temperature
    //var orderTemps = tempCollection.OrderBy(x => x.starttime).GroupBy(y => y.temp);
    //var tempPhrases = orderTemps.Select(x => $"From {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")}, at a temperature of {x.Key} and {x.Max(y => y.trend)}").ToObservableCollection();
    //var tempLine = $"{}" String.Join(", ", tempPhrases) + ". ";
    ////WindSpeed and Direction
    //var orderWinds = windCollection.OrderBy(x => x.starttime).GroupBy(y => y.windspeed);
    //var windsPhrases = orderWinds.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} {x.Max().direction} winds at speeds of {Math.Round(x.Key / 5) * 5} mph and {x.Max(y => y.trend)}").ToObservableCollection();
    //var windLine = String.Join(", ", windsPhrases) + ". ";
    ////Gusts
    //var orderGusts = gustsCollection.OrderBy(x => x.starttime).GroupBy(y => y.gust);
    //var gustPhrases = orderGusts.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} gusts up to {x.Key} mph and {x.Max(y => y.trend)}").ToObservableCollection();
    //var gustLine = String.Join(", ", gustPhrases) + ". ";
    ////Skycover
    //var orderClouds = skyCoverCollection.OrderBy(x => x.starttime).GroupBy(y => y.skycover);
    //var cloudPhrases = orderClouds.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} cloud coverage is {x.Key}% and {x.Max(y => y.trend)}").ToObservableCollection();
    //var cloudLine = String.Join(", ", cloudPhrases) + ". ";
    ////TStorms
    //var orderTstorms = tStormCollection.Where(r => r.tstorm != -1).OrderBy(x => x.starttime).GroupBy(y => y.tstorm);
    //var tstormPhrases = orderTstorms.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} there is a {x.Key}% chance of thunderstorms and damaging winds and {x.Max(y => y.trend)}").ToObservableCollection();
    //var tstormLine = String.Join(", ", tstormPhrases) + ". ";
    //string complete = firstLine + tstormLine + tempLine + cloudLine + windLine + gustLine;
    //return string.Empty;





    public string ProcessTrend(double currentValue, double futureValue)
    {
        try
        {

            if (currentValue < futureValue)
            {
                return "increasing";
            }
            if (currentValue > futureValue)
            {
                return "decreasing";
            }
            if (currentValue == futureValue)
            {
                return "stable";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR PROCESSING TREND{ex}");
            return "remains steady";
        }
        return "remains steady";
    }

    //public async Task<string> BuildStormStatementAsync(NwsHourlyForecast[] hourlies)
    //{

    //    if (hourlies.Length == 1) return hourlies[0].DetailedForecast;

    //    var weatherLayer = new ForecastLayers();
    //    var precipitationLayer = new ForecastLayers();
    //    var thunderLayer = new ForecastLayers();
    //    var skyCoverLayer = new ForecastLayers();
    //    var windLayer = new ForecastLayers();
    //    var heatIndexLayer = new ForecastLayers();
    //    var windChillLayer = new ForecastLayers();
    //    var temperatureLayer = new ForecastLayers();
    //    var weatherTimeLine = new StringBuilder();
    //    var min = hourlies.MinBy(x => x.StartTime);
    //    var max = hourlies.MaxBy(x => x.EndTime);
    //    var startTempStatement = TemperatureStatement(min.Temperature.Value, min.DewPoint.Value);
    //    var endTempStatement = TemperatureStatement(max.Temperature.Value, max.DewPoint.Value);
    //string tempTrend = ProcessTrend(min.Temperature.Value, max.Temperature.Value, min.StartTime, max.EndTime);



    //        weatherTimeLine.Append($"The {hourlies[0].TimeOfDay} of {hourlies[0].StartTime.ToString("dddd, MMMM d")} from {min.StartTime.ToString("h tt")} - {max.EndTime.ToString("h tt")}:");
    //        //Temperature and trend
    //        for(int t = 0; t < hourlies.Length-1; t++)
    //        {
    //            string tempTrend = ProcessTrend(hourlies[t].Temperature.Value, hourlies[t+1].Temperature.Value, hourlies[t].StartTime, hourlies[t+1].EndTime);
    //            if (hourlies[t].Temperature.Value != hourlies[t + 1].Temperature.Value)
    //            {
    //                weatherTimeLine.Append($"At {hourlies[t].StartTime:h tt} {hourlies[t].Temperature.Value} and {tempTrend}.");
    //            }
    ////        }

    //        if (hourlies[0].IsDaytime)
    //        {
    //            var storms = hourlies.SelectMany(x => x.Supplemental.Weather).ToList();
    //            var maxTempStatement = TemperatureStatement(max.Temperature.Value, max.Temperature.Value);
    //            var change = ProcessTrend(min.Temperature.Value, max.Temperature.Value, min.StartTime, max.EndTime);

    //            weatherTimeLine.Append($"");
    //        }
    //        var weatherItems = hourlies.Where(x => x.Supplemental != null && x.Supplemental.Weather != null)
    //    .SelectMany(forecast => forecast.Supplemental.Weather)
    //    .OrderBy(o => o.Time)
    //    .GroupBy(w => w.Category)
    //    .Select(group => new {
    //        start = group.Min(weather => weather.Time),
    //        duration = group.Sum(y => y.Duration),
    //        easiest = group.MinBy(x => x.Intensity switch
    //        {
    //            nameof(NwsWeatherIntensity.light) => 1,
    //            nameof(NwsWeatherIntensity.very_light) => 2,
    //            nameof(NwsWeatherIntensity.moderate) => 3,
    //            nameof(NwsWeatherIntensity.heavy) => 4,
    //            _ => 0
    //        }),
    //        hardest = group.MaxBy(x => x.Intensity switch
    //        {
    //            nameof(NwsWeatherIntensity.light) => 1,
    //            nameof(NwsWeatherIntensity.very_light) => 2,
    //            nameof(NwsWeatherIntensity.moderate) => 3,
    //            nameof(NwsWeatherIntensity.heavy) => 4,
    //            _ => 0
    //        }),
    //        mode = group.CountBy(s => s.v).OrderByDescending(x => x.Value).FirstOrDefault(),
    //        beginStorm = group.OrderBy(s => s.Time).FirstOrDefault(),
    //        endStorm = group.OrderByDescending(s => s.Time).FirstOrDefault());
    //    if (easiest != null && hardest != null)
    //    {
    //        return new ForecastLayers
    //        {
    //            LayerName = group.Key,
    //            Duration = duration,
    //            Min = new TimeAndQuantity
    //            {
    //                Intensity = easiest.Intensity,
    //                Time = easiest.Time
    //            },
    //            Max = new TimeAndQuantity
    //            {
    //                Intensity = hardest.Intensity,
    //                Time = hardest.Time
    //            },
    //            Mean = new TimeAndQuantity
    //            {
    //                Intensity = mode.Key
    //            },
    //            BeginStorm = new TimeAndQuantity
    //            {
    //                Intensity = beginStorm.Intensity,
    //                Time = beginStorm.Time
    //            },
    //            EndStorm = new TimeAndQuantity
    //            {
    //                Intensity = endStorm.Intensity,
    //                Time = endStorm.Time
    //            }
    //        };
    //    }
    //    else
    //    {
    //        return new ForecastLayers
    //        {
    //            LayerName = group.Key,
    //            Duration = duration,
    //            Min = easiest != null ? new TimeAndQuantity
    //            {
    //                Intensity = easiest.Intensity,
    //                Time = easiest.Time
    //            } : null,
    //            Max = hardest != null ? new TimeAndQuantity
    //            {
    //                Intensity = hardest.Intensity,
    //                Time = hardest.Time
    //            } : null,
    //            Mean = new TimeAndQuantity
    //            {
    //                Intensity = mode.Key
    //            }
    //        };

    //};
    //.OrderBy(x => x.BeginStorm.Time).ToArray();


    //    var precipitationData = hourlies
    //        .Select(x => x.QuantitativePrecipitation)
    //        .Where(precipitation => precipitation != null && precipitation.Value > 0)
    //        .OrderBy(p => p.Time)
    //        .ToList();

    //    if (precipitationData.Count > 0)
    //    {
    //        // Detect trends between consecutive hours
    //        var precipitationTrends = new List<(DateTime Time, double Value, string Trend)>();
    //        for (int i = 0; i < precipitationData.Count - 1; i++)
    //        {
    //            var current = precipitationData[i];
    //            var next = precipitationData[i + 1];
    //            var difference = next.Value - current.Value;

    //            var trend = Math.Abs(difference) < 0.1 ? "stable"
    //                      : difference > 0 ? "increasing"
    //                      : "decreasing";

    //            precipitationTrends.Add((current.Time, current.Value, trend));
    //        }

    //        // Group consecutive hours with the same trend
    //        var trendPeriods = new List<PrecipitationTrendPeriod>();
    //        if (precipitationTrends.Count > 0)
    //        {
    //            var currentPeriod = new PrecipitationTrendPeriod
    //            {
    //                Trend = precipitationTrends[0].Trend,
    //                StartTime = precipitationTrends[0].Time,
    //                EndTime = precipitationTrends[0].Time,
    //                Total = precipitationTrends[0].Value
    //            };

    //            for (int i = 1; i < precipitationTrends.Count; i++)
    //            {
    //                if (precipitationTrends[i].Trend == currentPeriod.Trend)
    //                {
    //                    // Same trend continues - extend the period
    //                    currentPeriod.EndTime = precipitationTrends[i].Time;
    //                    currentPeriod.Total += precipitationTrends[i].Value;
    //                }
    //                else
    //                {
    //                    // Trend changed - save current period and start a new one
    //                    trendPeriods.Add(currentPeriod);
    //                    currentPeriod = new PrecipitationTrendPeriod
    //                    {
    //                        Trend = precipitationTrends[i].Trend,
    //                        StartTime = precipitationTrends[i].Time,
    //                        EndTime = precipitationTrends[i].Time,
    //                        Total = precipitationTrends[i].Value
    //                    };
    //                }
    //            }
    //            trendPeriods.Add(currentPeriod);
    //        }

    //        // Build descriptive statements for each trend period
    //        var precipitationStatements = new List<string>();
    //        foreach (var period in trendPeriods)
    //        {
    //            var trendDesc = period.Trend switch
    //            {
    //                "increasing" => "intensifying",
    //                "decreasing" => "tapering off",
    //                "stable" => "remaining steady",
    //                _ => "changing"
    //            };

    //            var amountDesc = period.Total switch
    //            {
    //                < 0.1 => "less than a 1/10 of an inch",
    //                >= 0.1 and <= 0.2 => "between a 1/10 and 1/4 of an inch",
    //                >= 0.3 and <= 0.5 => "between 1/3 and 1/2 an inch",
    //                >= 0.6 and <= 0.7 => "between 2/3 and 3/4 of an inch",
    //                >= 0.8 and < 1 => "almost an inch",
    //                >= 1 and < 2 => $"{period.Total:0.0} inch",
    //                >= 2 => $"{period.Total:0.0} inches",
    //                _ => "no reported precipitation"
    //            };

    //            var statement = $"From {period.StartTime:h:mm tt} to {period.EndTime:h:mm tt}, expect about {amountDesc} of , {trendDesc}.";
    //            precipitationStatements.Add(statement);
    //        }

    //        precipitationLayer.LayerName = "Quantitative Precipitation";
    //        precipitationLayer.Total = precipitationData.Sum(x => x.Value);
    //        precipitationLayer.Mean = new TimeAndQuantity { Quantity = precipitationData.Average(x => x.Value) };
    //        precipitationLayer.Max = new TimeAndQuantity
    //        {
    //            Quantity = precipitationData.Max(x => x.Value),
    //            Time = precipitationData.MaxBy(x => x.Value).Time
    //        };
    //        precipitationLayer.Min = new TimeAndQuantity
    //        {
    //            Quantity = precipitationData.Min(x => x.Value),
    //            Time = precipitationData.MinBy(x => x.Value).Time
    //        };
    //        precipitationLayer.BeginStorm = new TimeAndQuantity
    //        {
    //            Time = precipitationData.First().Time,
    //            Quantity = precipitationData.First().Value
    //        };
    //        precipitationLayer.EndStorm = new TimeAndQuantity
    //        {
    //            Time = precipitationData.Last().Time,
    //            Quantity = precipitationData.Last().Value
    //        };
    //        precipitationLayer.Statement = string.Join(" ", precipitationStatements);
    //    }
    //    string progression = string.Empty;

    //    var weatherStatement = new StringBuilder();

    //    // weatherStatement.Append($"{}.LayerName} is moving into the {_session.NwsData.SelectedLocation.City} area around {weatherLayer.BeginStorm.Time.ToString("dddd MMMM d at h tt")}.");
    //    weatherStatement.Append($"{weatherLayer.LayerName} will start off {weatherLayer.BeginStorm.Intensity}.");
    //    weatherStatement.Append(precipitationLayer.Statement);
    //    return weatherStatement.ToString();
    //}

    //        else if (tempAvg > 30 && tempAvg< 60 && hours.Max(h => h.ProbabilityOfPrecipitation.Value) <= 12.0)
    //        {
    //            tempBuilder.Append($"With a {hours.Max(h => h.ProbabilityOfPrecipitation.Value)}% chance of precipitation and cool temperatures.");
    //        }
    //        else
    //        {
    //            string coldness = string.Empty;
    //var h = tempAvg;


    //tempBuilder.Append($" will make for a {coldness} {hours.FirstOrDefault().TimeOfDay}.");
    //        }
    //        return tempBuilder.ToString();
    //    }
    //var thunder = hourlies
    //    .Where(x => x.Supplemental != null && x.Supplemental.ProbabilityOfThunder != null && x.Supplemental.ProbabilityOfThunder.Value >= 10)
    //    .Select(forecast => new { Time = forecast.Supplemental.ProbabilityOfThunder.Time, EndTime = forecast.Supplemental.ProbabilityOfThunder.Time.AddHours(forecast.Supplemental.ProbabilityOfThunder.Duration), Value = forecast.Supplemental?.ProbabilityOfThunder?.Value, Description = forecast.Supplemental?.ProbabilityOfThunder?.Description });
    //var thunderLayer = new ForecastLayers
    //{
    //    LayerName = "Thunder",
    //    Min = new TimeAndQuantity
    //    {
    //       Quantity =thunder.Where(y=>y.Value.HasValue).Min(x=>x.Value.Value),
    //       Time = thunder.Where(y=>y.Value.HasValue).MinBy(x=>x.Value).Time
    //    },
    //    Max= new TimeAndQuantity
    //    {
    //        Quantity = thunder.Where(y => y.Value.HasValue).Max(x => x.Value.Value),
    //        Time = thunder.Where(y => y.Value.HasValue).MaxBy(x => x.Value).Time
    //    },
    //    Mean = new TimeAndQuantity
    //    {
    //        Quantity = thunder.OrderBy(x=>x.Value).Count() / 2 = 0 Where(y => y.Value.HasValue).in(x => x.Value.Value),
    //        Time = thunder.Where(y => y.Value.HasValue).MinBy(x => x.Value).Time
    //    },

    //    var precipitation = hourlies
    //    .Where(x => x.Supplemental != null && x.Supplemental.QuantitativePrecipitation != null && x.Supplemental.QuantitativePrecipitation.Value > 0)
    //    .Select(value => value!.Supplemental!.QuantitativePrecipitation!.Value)
    //    .DefaultIfEmpty()
    //    .Sum();




    //if ((thunder != null && thunder.Value >= 10) || precipitation > 0)
    //{
    //    statement.Append("⚠️ LOOKOUT! There is weather afoot!.. ");

    //    if (thunder != null && thunder.Value >= 10)
    //    {
    //        statement.Append(thunder.Description);
    //    }

    //    if ((thunder != null && thunder.Value >= 10) && precipitation > 0)
    //    {
    //        statement.Append(" and ");
    //    }

    //    if (precipitation > 0)
    //    {
    //        statement.Append($"{precipitationStatement} of precipitation");
    //    }

    //    statement.Append(". ");
    //}

    //if (weatherItems.Length > 0)
    //{
    //    statement.Append($"During the {timeOfDay}, expect ");

    //    for (var index = 0; index < weatherItems.Length; index++)
    //    {
    //        if (index > 0)
    //        {
    //            statement.Append(index == weatherItems.Length - 1
    //                ? " and "
    //                : ", ");
    //        }

    //        var item = weatherItems[index];
    //        statement.Append(
    //            $"{item.Condition} from {item.StartTime:h tt} to {item.EndTime:h tt}");
    //    }

    //    statement.Append(". ");
    //}

    //var minimum = hourlies.MinBy(forecast => forecast.Temperature.Value);
    //var maximum = hourlies.MaxBy(forecast => forecast.Temperature.Value);

    //if (minimum != null && maximum != null)
    //{
    //    statement.Append(
    //        $"Temperatures range from {minimum.TempAndUnit} to {maximum.TempAndUnit}.");
    //}

    //return statement.ToString().Trim();

    //private ForecastItem[] BuildWeatherItems(IEnumerable<NwsHourlyForecast> forecasts)
    //{
    //    return forecasts
    //        .SelectMany(forecast => forecast.Supplemental?.Weather ?? [])
    //        .Where(weather => weather != null &&
    //                          !string.IsNullOrWhiteSpace(weather.Category))
    //        .GroupBy(
    //            weather => weather.Category,
    //            StringComparer.OrdinalIgnoreCase)
    //        .Select(group =>
    //        {
    //            var start = group.Min(weather => weather.Time);
    //            var duration = group.FirstOrDefault().Duration;
    //            var end = start.AddHours(duration);


    //            return new ForecastItem
    //            {
    //                Condition = group.Key,
    //                StartTime = start,
    //                EndTime = end == start ? start.AddHours(1) : end
    //            };
    //        })
    //        .OrderBy(item => item.StartTime)
    //        .ToArray();
    //}



    //weatherStatement.Append($"⚠️ HEAD'S UP! THERE'S SOME WEATHER AFOOT!{Environment.NewLine}");


    //var windsGroup = hourlies.GroupBy(x => (int)(Math.Round(x.WindSpeed.Value / 5)) * 5).ToArray();

    //for (int n = 0; n < windsGroup.Length; n++)
    //{
    //    int speed = windsGroup[n].Key;
    //    string intensity = speed switch
    //    {
    //        >= 0 and <= 1 => "Calm winds",
    //        >= 1 and <= 3 => "Light winds",
    //        >= 4 and <= 7 => "light breeze",
    //        >= 8 and <= 12 => "Gentle breeze",
    //        >= 13 and <= 18 => "Moderate breeze",
    //        >= 19 and <= 24 => "Fresh breeze",
    //        >= 25 and <= 31 => "Strong breeze",
    //        >= 32 and <= 38 => "Near gale force winds",
    //        >= 39 and <= 46 => "Gale force winds",
    //        >= 47 and <= 54 => "Severe gale force winds",
    //        >= 55 and <= 63 => "Storm winds",
    //        >= 64 and <= 72 => "Violent storm winds",
    //        >= 73 => "Hurricane force winds",
    //        _ => ""
    //    };
    //    var startTime = windsGroup[n].Min(s => s.StartTime);
    //    var endTime = windsGroup[n].Max(s => s.EndTime);
    //    var domDirection = windsGroup[n].CountBy(s => s.WindDirection).Max(t => t.Key);
    //    var minGust = windsGroup[n].Min(s => s.WindGust?.Value);
    //    var maxGust = windsGroup[n].Max(s => s.WindGust?.Value);
    //    windStatement.Append($"{intensity} from {startTime:htt} to {endTime:htt} blowing mostly from the {domDirection} at {speed}mph");
    //    string gusts = minGust != null && maxGust != null && minGust != maxGust ? $", and gusts from {minGust} to {maxGust}mph." : minGust != null && maxGust != null && minGust == maxGust ? $", and gusts at {maxGust}mph." : ".";
    //    windStatement.Append(gusts);



    private string BuildStormAndWarningStatement(List<ValueEvolution> evolutions)
    {
        var weatherStatements = _session.NwsData.HourlySupplementals.OrderBy(o => o.Time).ToArray();
        if (weatherStatements != null)
        {
            var thunderKeywords = new[] { "thunder", "thunderstorm", "severe", "tstm" };
            var stormHours = weatherStatements.Where(e => e.Weather.Select(x => x.Statement) != null)
                                        .OrderBy(e => e.Time)
                                        .ToArray();

            if (stormHours.Length == 0) return string.Empty;

            var start = stormHours.First().Time;
            var end = stormHours.Last().Time == default ? stormHours.Last().Time.AddHours(1) : stormHours.Last().Time;
            var msg = new StringBuilder();
            msg.Append($"⚠️ Severe weather possible from {start:h tt} to {end:h tt}. ");
            if (_session.NwsData.HourlySupplementals.Where(x => x.Time.Date == start.Date).Select(t => t.ProbabilityOfThunder?.Description).ToArray() is string[] tstormDescription)
            {
                //From x to x there is a moderate change of thrudersstorms.
                msg.Append("Prepare for a moderate chance of thunderstorms and strong winds.");
            }
            // Use the first matching statement for detail if present
            var detail = stormHours.Select(e => e.SupplementalWeatherStatement).FirstOrDefault(s => !string.IsNullOrEmpty(s));
            if (!string.IsNullOrEmpty(detail)) msg.Append(detail + ". ");

            return msg.ToString();

        }
        return "no storms to report.";
    }

    private IEnumerable<string> BuildPrecipitationStatements(List<ValueEvolution> evolutions)
    {
        var results = new List<string>();
        if (evolutions == null || !evolutions.Any()) return results;

        var precipHours = evolutions.Where(e => !string.IsNullOrEmpty(e.PrecipitationType) && !e.PrecipitationType.Equals("none", StringComparison.OrdinalIgnoreCase) || e.PrecipitationAmt > 0)
                                    .OrderBy(e => e.Time)
                                    .ToArray();
        if (!precipHours.Any()) return results;

        // group contiguous hours
        var groups = new List<List<ValueEvolution>>();
        List<ValueEvolution> current = null;
        foreach (var h in precipHours)
        {
            if (current == null) { current = new List<ValueEvolution> { h }; continue; }
            var prev = current.Last();
            if ((h.Time - prev.Time).TotalHours <= 1.5) current.Add(h);
            else { groups.Add(current); current = new List<ValueEvolution> { h }; }
        }
        if (current != null) groups.Add(current);

        foreach (var g in groups)
        {
            var start = g.First().Time;
            var end = g.Last().EndTime == default ? g.Last().Time.AddHours(1) : g.Last().EndTime;
            var type = g.GroupBy(x => x.PrecipitationType ?? "none").OrderByDescending(gr => gr.Count()).First().Key;
            var total = g.Sum(x => x.PrecipitationAmt);
            var stmt = new StringBuilder();
            stmt.Append($"Expect {type} from {start:h tt} to {end:h tt}");
            stmt.Append(GetPrecipitationStatement(type, total));
            results.Add(stmt.ToString());
        }

        return results;
    }

    private string BuildBriefForecastStatement(List<ValueEvolution> evolutions)
    {
        if (evolutions == null || !evolutions.Any()) return string.Empty;
        var list = evolutions.OrderBy(e => e.Time).Select(e => (Time: e.Time, desc: e.WeatherDescription?.FirstOrDefault()?.Value ?? e.ShortForecast?.FirstOrDefault()?.Value ?? string.Empty)).ToList();            // collapse consecutive equal descriptions
        var collapsed = new List<(DateTime Start, DateTime End, string Desc)>();
        DateTime start = list[0].Time;
        DateTime end = list[0].Time.AddHours(1);
        string descCur = list[0].desc;
        for (int i = 1; i < list.Count; i++)
        {
            var item = list[i];
            if (string.Equals(item.desc, descCur, StringComparison.OrdinalIgnoreCase))
            {
                end = item.Time.AddHours(1);

            }
            else
            {
                collapsed.Add((start, end, descCur));
                start = item.Time;
                end = item.Time.AddHours(1);
                descCur = item.desc;
            }
        }
        collapsed.Add((start, end, descCur));

        if (!collapsed.Any()) return string.Empty;
        var sb = new StringBuilder();
        sb.Append("Brief: ");
        for (int i = 0; i < collapsed.Count; i++)
        {
            var c = collapsed[i];
            sb.Append($"{c.Desc} from {c.Start:h tt} to {c.End:h tt}");
            if (i < collapsed.Count - 2) sb.Append(", ");
            else if (i == collapsed.Count - 2) sb.Append(" and ");
            else sb.Append(".");
        }
        return sb.ToString();
    }

    //public List<ForecastItem> GetPrecipitation(NwsHourlyForecast[] hourlies, IEnumerable<ForecastItem> forecastItems)
    //{
    //    List<ForecastItem> items = new();

    //    var precipitation = hourlies.Where(x => x.Supplemental?.QuantitativePrecipitation != null &&
    //                                            x.Supplemental.QuantitativePrecipitation.Value >= 0 &&
    //                                            forecastItems.Select(w => w.StartTime.CompareTo(x.Supplemental.QuantitativePrecipitation.Time) == 0).Any());
    //    var totalPrecipitation = precipitation.Where(y => y.ProbabilityOfPrecipitation.Value > 0).Sum(x => x.Supplemental.QuantitativePrecipitation.Value);

    //    string precipitationStatement = totalPrecipitation switch
    //    {
    //        >= 0.1 and <= 0.2 => "between a tenth and a quarter of an inch",
    //        >= .3 and <= .5 => "between 1/3 and half an inch",
    //        >= .6 and <= .7 => "between 2/3 and three quarters of an inch",
    //        >= .8 and < 1 => "almost an inch",
    //        >= 1 and < 2 => $"{totalPrecipitation} inch",
    //        >= 2 => $"{totalPrecipitation} inches",
    //        _ => "no reported precipitation"
    //    };
    //    var precipitationItem = new ForecastItem
    //    {
    //        Order = 3,
    //        Condition = precipitationStatement,
    //        StartTime = precipitation.Min(x => x.StartTime),
    //        EndTime = precipitation.Max(x => x.EndTime)
    //    };
    //    forecastItems.Add(precipitationItem);
    //}

    // Hi and Low statement for 24 hour period


    private double ParseWindSpeed(double Value) => Value; // placeholder for unit conversion if needed


    public async Task<ObservableCollection<string>> BuildDailyForecastAsync(NwsTimeOfDayForecast tod)
    {
        try
        {
            Debug.WriteLine("BuildDailyForecastAsync: start");

            var hourlies = tod.Forecasts;
            Debug.WriteLine($"BuildDailyForecastAsync: loaded {hourlies.Length} nws hours. StartTime={tod.StartTime:f} EndTime:{tod.EndTime:f}");

            // Map to ValueEvolution on background thread
            List<ValueEvolution> evolutions = await BuildEvolutionListsAsync(hourlies);

            Debug.WriteLine($"BuildDailyForecastAsync: mapped {evolutions.Count} evolutions");

            // Build and smooth segments on background
            var segments = await Task.Run(() => ForecastSegmentation.BuildSegments(evolutions));
            var smoothed = await Task.Run(() => ForecastSegmentation.SmoothSegments(segments, minHours: 2));

            Debug.WriteLine($"BuildDailyForecastAsync: built {smoothed.Count} segments");

            // Build statements on background, then marshal ObservableCollection creation to UI thread
            var statementList = await Task.Run(async () =>
            {
                var list = new List<string>();
                var stormStmt = await BuildTimeOfDayForecastsAsync(_session.NwsData.HourlyForecasts);
                // if (!string.IsNullOrEmpty(stormStmt))
                // {
                //   list.Add(stormStmt);
                list.AddRange(BuildPrecipitationStatements(evolutions));
                //}
                var brief = BuildBriefForecastStatement(evolutions);
                if (!string.IsNullOrEmpty(brief)) list.Add(brief);
                var tempStmt = TemperatureStatement(evolutions.FirstOrDefault().Temperature.Select(y => y.Value).Max(), evolutions.FirstOrDefault().DewPoints.Select(y => y.Value).Max(), evolutions.FirstOrDefault().Precipitation.Select(y => y.Amount).Max());
                if (!string.IsNullOrEmpty(tempStmt)) list.Add(tempStmt);
                //var skyStmt = BuildSkyCoverStatement(evolutions);
                //if (!string.IsNullOrEmpty(skyStmt)) list.Add(skyStmt);
                //list.AddRange(BuildWindStatements(evolutions));
                if (!list.Any())
                {
                    foreach (var s in smoothed)
                        list.Add($"{s.Start:h tt} to {s.End:h tt}: {Math.Round(s.Representative.Temperature)}°, {s.Representative.PrecipitationType}");
                }
                var forecasts = new ObservableCollection<string>(list);
                return forecasts;
            });

            Debug.WriteLine($"BuildDailyForecastAsync: built {statementList.Count} statements");

            // Marshal ObservableCollection creation to UI thread
            var statements = new ObservableCollection<string>(statementList);
            //string paragraph = String.Join(" ", statements);

            Debug.WriteLine("BuildDailyForecastAsync: completed successfully");
            return statements;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BuildDailyForecastAsync: exception - {ex}");
            Debug.WriteLine($"Error building forecast: {ex.Message}");
        }

        return new ObservableCollection<string>();
    }

    private async Task<List<ValueEvolution>> BuildEvolutionListsAsync(NwsHourlyForecast[] localNws)
    {
        var evolutions = await Task.Run(async () =>
        {
            var evolutions = new List<ValueEvolution>();
            var weather = new ObservableCollection<WeatherStatementsValue>();
            var stormBuilder = new StringBuilder();
            //var dayHourlies = localNws.Where(h => h.StartTime.Time == localSelectedDate).OrderBy(h => h.StartTime).ToArray();
            foreach (var nws in localNws)
            {
                // Transform the array weather statements into a single statement.
                weather.Clear();
                stormBuilder.Clear();
                if (nws.Supplemental != null && nws.Supplemental.Weather != null && nws.Supplemental.Weather.Length > 0)
                {
                    for (int w = 0; w < nws.Supplemental?.Weather.Length; w++)
                    {
                        weather.Add(nws.Supplemental?.Weather[w]);
                    }
                    for (int w = 0; w < weather.Count; w++)
                    {
                        if (w == 0)
                        {
                            stormBuilder.Append($"Expect {GetNwsData.CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity ?? "")}");
                        }
                        if (w == 1)
                        {
                            stormBuilder.Append($"along with {GetNwsData.CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity) ?? ""}");
                        }
                        if (w > 1)
                        {
                            stormBuilder.Append($",{GetNwsData.CoverageStmt(weather[w].Coverage, weather[w].Weather, weather[w].Intensity ?? "")}");
                        }
                        if (weather.Count - 1 == w)
                        {
                            stormBuilder.Append($" lasting about {weather[w].Duration} hours.");
                        }
                    }
                }
                //            string precipicationType = GetPrecipitationType(nws);
                var evo = new ValueEvolution
                {
                    Time = nws.StartTime,
                    EndTime = nws.EndTime,
                    IsDay = nws.IsDaytime,
                    Temperature = new List<ForecastTemperature> { new ForecastTemperature { StartTime = nws.StartTime, Value = nws.Temperature.Value, UnitCode = nws.Temperature.UnitCode } },
                    ApparentTemperature = new List<ForecastApparentTemperature> { new ForecastApparentTemperature { StartTime = nws.StartTime, Value = nws.Supplemental?.ApparentTemperature?.Value ?? 0, UnitCode = nws.Supplemental.ApparentTemperature?.UnitCode ?? string.Empty } },
                    Precipitation = new List<ForecastPrecipitation> { new ForecastPrecipitation { StartTime = nws.StartTime, Type = nws.Supplemental?.Weather?[0]?.Category ?? "none", Value = nws.Supplemental?.ProbabilityOfPrecipitation?.Value ?? 0.0D, Amount = nws.Supplemental?.QuantitativePrecipitation?.Value ?? 0.0D } },
                    Wind = new List<ForecastWind> { new ForecastWind { StartTime = nws.StartTime, Value = nws.WindSpeed.Value, Direction = nws.WindDirection } },
                    Gusts = new List<ForecastGusts> { new ForecastGusts { StartTime = nws.StartTime, Value = nws.WindGust != null ? nws.WindGust.Value : 0.0D } },
                    SkyCover = new List<ForecastSkyCover> { new ForecastSkyCover { StartTime = nws.StartTime, Value = nws.Supplemental?.Skycover?.Value ?? 0.0D } },
                    MixingHeight = new List<ForecastMixingHeight> { new ForecastMixingHeight { StartTime = nws.StartTime, Value = nws.Supplemental?.MixingHeight?.Value ?? 0.0D } },
                    Wbgt = new List<ForecastWbgt> { new ForecastWbgt { StartTime = nws.StartTime, Value = nws.Supplemental?.WetBulbGlobeTemperature?.Value ?? 0.0D } },
                    HeatRisk = new List<ForecastHeatRisk> { new ForecastHeatRisk { StartTime = nws.StartTime, Value = nws.Supplemental?.HeatRisk?.Value ?? 0 } },
                    WindChill = new List<ForecastWindChill> { new ForecastWindChill { StartTime = nws.StartTime, Value = nws.Supplemental?.WindChill?.Value ?? 0.0D } },
                    ShortForecast = new List<ForecastShortForecast> { new ForecastShortForecast { StartTime = nws.StartTime, Value = nws.ShortForecast } },
                    WeatherDescription = new List<ForecastWmoDescription> { new ForecastWmoDescription { StartTime = nws.StartTime, Value = nws.DetailedForecast } },
                    Thunder = new List<ForecastThunder> { new ForecastThunder { StartTime = nws.StartTime, Value = nws.Supplemental?.ProbabilityOfThunder.Value ?? 0.0D } },
                    WeatherStatement = new List<ForecastWeatherStatement> { new ForecastWeatherStatement { StartTime = nws.StartTime, Value = stormBuilder.ToString() ?? "" } },
                    IceAccumulation = new List<ForecastIceAccumulation> { new ForecastIceAccumulation { StartTime = nws.StartTime, Value = nws.Supplemental?.IceAccumulation.Value ?? 0.0D } },
                    SnowfallAmount = new List<ForecastSnowfallAmount> { new ForecastSnowfallAmount { StartTime = nws.StartTime, Value = nws.Supplemental?.SnowfallAmount.Value ?? 0.0D } }
                };


                evolutions.Add(evo);
            }
            return evolutions;
        });
        return evolutions;
    }

    public string TemperatureStatement(double t, double d, double q)
    {
        var tempBuilder = new StringBuilder();
        var temp = string.Empty;
        if (t < 30) temp = "below freezing";
        if (30 <= t && t < 34) temp = "in the low 30's";
        if (34 <= t && t <= 36) temp = "in the mid 30's";
        if (37 <= t && t < 40) temp = "in the high 30's";
        if (40 <= t && t < 44) temp = "in the low 40's";
        if (44 <= t && t <= 46) temp = "in the mid 40's";
        if (47 <= t && t < 50) temp = "in the high 40's";
        if (50 <= t && t < 54) temp = "in the low 50's";
        if (54 <= t && t <= 56) temp = "in the mid 50's";
        if (57 <= t && t < 60) temp = "in the high 50's";
        if (60 <= t && t < 64) temp = "in the low 60's";
        if (64 <= t && t <= 66) temp = "in the mid 60's";
        if (67 <= t && t < 70) temp = "in the high 60's";
        if (70 <= t && t < 74) temp = "in the low 70's";
        if (74 <= t && t <= 76) temp = "in the mid 70's";
        if (77 <= t && t < 80) temp = "in the high 70's";
        if (80 <= t && t < 84) temp = "in the low 80's";
        if (84 <= t && t <= 86) temp = "in the mid 80's";
        if (87 <= t && t < 90) temp = "in the high 80's";
        if (90 <= t && t < 94) temp = "in the low 90's";
        if (94 <= t && t <= 96) temp = "in the mid 90's";
        if (97 <= t && t < 100) temp = "in the high 90's";
        if (100 <= t && t < 104) temp = "in the low 100's";
        if (104 <= t && t <= 106) temp = "reaching about 105";
        if (107 <= t && t < 110) temp = "climbing near 110";
        if (t >= 110) temp = "reaching over 110";
        tempBuilder.Append($"With temperature {temp}");

        var feelsLike = string.Empty;

        string dp = string.Empty;
        if (d < 30) dp = "less than 30";
        if (30 <= d && d < 34) dp = "in the low 30's,";
        if (34 <= d && d <= 36) dp = "in the mid 30's,";
        if (37 <= d && d < 40) dp = "in the high 30's";
        if (40 <= d && d < 44) dp = "in the low 40's";
        if (44 <= d && d <= 46) dp = "in the mid 40's,";
        if (47 <= d && d < 50) dp = "in the high 40's,";
        if (50 <= d && d < 54) dp = "in the low 50's";
        if (54 <= d && d <= 56) dp = "in the mid 50's,";
        if (57 <= d && d < 60) dp = "in the high 50's";
        if (60 <= d && d < 64) dp = "in the low 60's,";
        if (64 <= d && d <= 66) dp = "in the mid 60's,";
        if (67 <= d && d < 70) dp = "in the high 60's";
        if (70 <= d && d < 75) dp = "in the low 70's";
        if (d > 75) dp = $" at {d}";
        tempBuilder.Append($" and dew point {dp},");
        string precipitation = string.Empty;
        if (q > 0)
        {
            precipitation = "damp and ";
        }
        else
        {
            precipitation = "";
        }

        string coldness = "";

        switch (t)
        {
            case >= 30 and < 40:
                coldness = "brisk";
                break;
            case > 20 and < 30:
                coldness = "cold";
                break;
            case > 10 and <= 20:
                coldness = "chilly";
                break;
            case <= 10:
                coldness = "frigid";
                break;
        }

        switch (d)
        {
            case (<= 30):
                feelsLike = "";
                break;
            case (> 30 and <= 40):
                feelsLike = "cool and dry";
                break;
            case (> 40 and <= 50):
                feelsLike = "cool and pleasant";
                break;
            case (> 50 and <= 60) when t >= 60 && t < 65:
                feelsLike = "to those that are stationary, cool and mostly comfortable. To those that are mobile, raw and humid";
                break;
            case (> 50 and <= 60) when t > 65:
                feelsLike = "cool and comfortable";
                break;
            case (> 60 and <= 70) when t >= 70 && t <= 75:
                feelsLike = "warm and humid";
                break;
            case (> 60 and <= 70) when t > 75:
                feelsLike = "warm and mostly comfortable";
                break;
            case > 70 and <= 75:
                feelsLike = "hot and humid";
                break;
            case > 75:
                //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
                feelsLike = "tropical";
                break;
        }
        var refined = coldness + precipitation + feelsLike;
        var splitString = refined.Split(" and ");
        if (splitString.Count() > 2)
        {
            refined = $"{String.Join(", ", splitString.SkipLast(1))} and {splitString.Last()}";
        }
        tempBuilder.Append($", will make the great outdoors feel {refined}.");

        return tempBuilder.ToString();
    }

    private static string GetPrecipitationType(NwsHourlyForecast nws)
    {
        var precipicationType = string.Empty;
        if (nws.Supplemental?.Weather != null)
        {
            return nws.Supplemental.SupplementalWeatherStatement;
        }
        return "none";
    }
    private void SetTimesAndProperties(ValueEvolution[] dailyEvolution)
    {
        for (int e = 1; e < dailyEvolution.Length; e++)
        {
            if (dailyEvolution[e].WindSpeed == dailyEvolution[e - 1].WindSpeed)
            {

            }
        }
    }

    private string GetForecastStatements(ValueEvolution evolution)
    {
        if (evolution == null) return string.Empty;
        // Build a single-line summary from the ValueEvolution lists
        var temp = evolution.Temperature?.FirstOrDefault()?.Value ?? 0;
        var heat = evolution.ApparentTemperature?.FirstOrDefault()?.Value ?? 0;
        var dewpoint = evolution.DewPoints?.FirstOrDefault()?.Value ?? 0;
        var wbgt = evolution.Wbgt?.FirstOrDefault()?.Value ?? 0;
        var relativeHumidity = evolution.RelativeHumidity?.FirstOrDefault()?.Value ?? 0;
        var sky = evolution.SkyCover?.FirstOrDefault()?.Value ?? 0;
        var wind = evolution.Wind?.FirstOrDefault()?.Value ?? 0;
        var gust = evolution.Gusts?.FirstOrDefault()?.Value ?? 0;

        var precipType = evolution.PrecipitationType ?? "none";
        var precipAmt = evolution.PrecipitationAmt;
        var weatherDesc = string.Join(", ", (evolution.WeatherDescription ?? new List<ForecastWmoDescription>()).Select(w => w?.Value ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)));
        var weatherStmt = evolution.ShortForecast?.FirstOrDefault()?.Value ?? string.Empty;

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(weatherStmt)) sb.Append(weatherStmt + ". ");
        sb.Append(GetPrecipitationStatement(precipType, precipAmt));
        if (!string.IsNullOrEmpty(weatherDesc)) sb.Append(weatherDesc + ". ");
        sb.Append($"{temp} {TempCategory(temp, dewpoint, relativeHumidity, wbgt)}. ");
        sb.Append(CalcCloudCoverage(sky, evolution.IsDay));
        sb.Append(CalcWind(wind));
        sb.Append(GustSpeed(gust));
        return sb.ToString();
    }
    // Lans P. Rothfusz and described in a 1990 National Weather Service (NWS) Technical Attachment (SR 90-23)


    private string GetPrecipitationStatement(string precipitationType, double precipitationAmount)
    {
        if (precipitationAmount > 0)
        {
            var precipitation = Math.Round(precipitationAmount, 3) switch
            {
                < 0.1 => ", totaling less than a tenth of an inch.",
                >= 0.1 and <= 0.25 => ", totaling about 1/4 of an inch.",
                > 0.25 and <= 0.50 => ", totaling about 1/2 of an inch.",
                > 0.50 and <= 0.75 => ", totaling about 3/4 of an inch.",
                > 0.75 and <= 1.0 => ", totaling about an inch.",
                > 1.0 => $", totaling about {Math.Round(precipitationAmount, 0)} inches.",
                _ => ", there is no precipation to report."
            };
            precipitation = precipitationType + precipitation;
            return precipitation;
        }
        else
        {
            return ".";
        }
        return string.Empty;
    }

    // 1 Looking up,the sky feels like it's falling and mmostly cloud.
    private string CalcCloudCoverage(double skyCover, bool isDay)
    {
        string cloudConditions = string.Empty;

        switch (skyCover)
        {
            case < 12.5:
                cloudConditions += (bool)isDay ? "sunny" : "clear night";
                break;
            case < 37.5:
                cloudConditions += (bool)isDay ? "mostly sunny" : "mostly clear";
                break;
            case < 62.5:
                cloudConditions += (bool)isDay ? "partly sunny" : "partly cloudy";
                break;
            case < 87.5:
                cloudConditions += "mostly cloudy";
                break;
            case <= 100:
                cloudConditions += "cloudy";
                break;
            default:
                break;
        }

        return cloudConditions;
    }

    private string CalcCeilingHeight(double ceilingHeight, double skyCover)
    {
        string ceiling = ceilingHeight switch
        {
            > 6500 => "high cloud ceiling — sky feels open",
            > 3000 and <= 6500 => "Moderate ceiling — partial obstruction",
            > 1000 and <= 3000 => "Low ceiling — sky feels closed‑in",
            < 1000 => "Low ceiling - gloomy",
            _ => "High ceiling - sky feels open"
        };
        return ceiling;
    }
    // 2 //3 


    private string CalcWind(double wind)
    {
        double speed = wind;


        // Beaufort scale classification (mph)
        return speed switch
        {
            >= 0 and <= 1 => "Calm",
            >= 1 and <= 3 => "Light Air",
            >= 4 and <= 7 => "Light Breeze",
            >= 8 and <= 12 => "Gentle Breeze",
            >= 13 and <= 18 => "Moderate Breeze",
            >= 19 and <= 24 => "Fresh Breeze",
            >= 25 and <= 31 => "Strong Breeze",
            >= 32 and <= 38 => "Near Gale",
            >= 39 and <= 46 => "Gale",
            >= 47 and <= 54 => "Severe Gale",
            >= 55 and <= 63 => "Storm",
            >= 64 and <= 72 => "Violent Storm",
            >= 73 => "Hurricane",
            _ => null
        };
    }

    private string GustSpeed(double gustSpeed)
    {
        return gustSpeed switch
        {
            >= 0 and <= 3 => "No wind gusts to report.",
            >= 4 and <= 12 => "Gusts capable of making the leaves on a tree shake.",
            >= 13 and <= 24 => "Gusts capable of moving small branches and raise loose dust.",
            >= 25 and <= 38 => "Gusts capable of causing distrubences that may require professional attention; whole trees sway.",
            >= 39 and <= 54 => "Gusts capable of breaking twigs and may cause noticeable damage.",
            >= 55 => "Destructive gusts capable of widespread damage.",
            _ => "No wind gusts to report."
        };
    }

    private string CreateHourlyDetailDescriptionAsync(NwsHourlyForecast hourly, NwsHourlySupplemental? hourlyExtra)
    {
        string feelsLike = string.Empty;
        string storm = string.Empty;
        string weather = string.Empty;
        string windGusts = string.Empty;
        string skyCover = string.Empty;
        string adjustedTemp = string.Empty;
        string thunder = string.Empty;
        string temperatureCloudCoverApparentTemp = string.Empty;
        if (hourlyExtra != null)
        {

            if (hourlyExtra?.SupplementalWeatherStatement != null)
            {
                weather = hourlyExtra.SupplementalWeatherStatement;
            }



            if (hourlyExtra?.Skycover?.Value != null)
            {
                var skyCoverValue = hourlyExtra?.Skycover?.Value;
                if (skyCoverValue <= 10) skyCover = " Clear sky, ";
                if (skyCoverValue <= 25) skyCover = " Only a few clouds, ";
                if (skyCoverValue <= 50) skyCover = " Only some breaks in the clouds, ";
                if (skyCoverValue <= 75) skyCover = " Mostly cloudy, ";
                skyCover = "";
            }




            if (weather != string.Empty) storm = weather;

            if (thunder != string.Empty && !(storm.Contains("thunder"))) storm += $" {thunder} ";



            //Part 2 Done Storm/conditions
            if (!(storm.ToLowerInvariant().Contains(hourly.ShortForecast.ToLowerInvariant())))
            {
                storm += hourly.ShortForecast.ToLowerInvariant();

            }
        }
        if (storm == string.Empty) storm = hourly.ShortForecast.ToLowerInvariant() + ", ";

        if (hourly.Temperature.Value > 70)
        {
            var h = hourly.Temperature.Value;
            switch (hourly.DewPoint.Value)
            {
                case <= 50:
                    feelsLike = "pleasant";
                    break;
                case > 50 and <= 60:
                    feelsLike = "comfortable.";
                    break;
                case (> 60 and <= 70) when h >= 70 && h < 75:
                    feelsLike = "humid";
                    break;
                case (> 60 and <= 70) when h > 75:
                    feelsLike = "mostly conformtable.";
                    break;
                case > 70 and <= 75:
                    feelsLike = "humid.";
                    break;
                case > 75:
                    //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
                    feelsLike = "tropical.";
                    break;
            }
        }



        string openerWelcome = hourly.IntroTimeOfDay;
        string openerLine = "! Here's The Photographer's Forecast for ";
        string openerTime = $"  {hourly.StartTime.ToString("h tt")} to {hourly.EndTime.ToString("h tt")}: ";
        // Part 1 Done Opener
        string opener = $"{openerWelcome}{openerLine}{openerTime}";
        string apparentTemp = hourlyExtra?.ApparentTemperature?.Value != null ? $"{Math.Round((double)hourlyExtra.ApparentTemperature.Value * 1.8 + 32, 0)}°F" : string.Empty;

        string temp = $" and {hourly.Temperature.Value}{hourly.Temperature.UnitCode}.";
        string wind = $" Winds from the {GetNwsData.GetWindDirection(hourly.WindDirection).ToLowerInvariant()} at {hourly.WindSpeed}{(windGusts != string.Empty ? windGusts : ".")}";
        string dewpoint = $"Dew point {Services.NumberGroups.DewPointToString(hourly.DewPoint.Value)} ";
        string apparent = apparentTemp != string.Empty ? $" makes the outdoor temperature feel like {apparentTemp}." : ".";


        return $"{opener}{storm}{temp}{wind}";
    }

    // Basic stats on a bin
    public double Mean(Bin bin) =>
        bin.Values.Any() ? bin.Values.Average() : 0;

    public double Min(Bin bin) =>
        bin.Values.Any() ? bin.Values.Min() : 0;

    public double Max(Bin bin) =>
        bin.Values.Any() ? bin.Values.Max() : 0;

    // Trend between two bins
    public string Trend(double previousMean, double currentMean)
    {
        double delta = currentMean - previousMean;

        if (delta >= 0.30) return "rapidly increasing";
        if (delta >= 0.15) return "increasing";
        if (delta <= -0.30) return "rapidly decreasing";
        if (delta <= -0.15) return "decreasing";
        return "remaining fairly steady";
    }
}


public class PrecipitationTrendPeriod
{
    public string Trend { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Total { get; set; }
}


