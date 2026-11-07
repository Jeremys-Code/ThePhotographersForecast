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
    //{If WeatherStatement Exists}
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
    public async Task<NwsTimeOfDayForecast[]> BuildTimeOfDayForecastsAsync(NwsDailyForecast[] dailies)
    {
        var timeOfDayCollection = new ObservableCollection<NwsTimeOfDayForecast>();

        for (int i = 0; i < dailies.Length; i++)
        {


            var hourlies = dailies[i].HourlyWeathers.ToArray();
            var timesOfDayHours = hourlies.OrderBy(x => x.StartTime).GroupBy(x => x.TimeOfDay);
            Debug.WriteLine($"There are {timesOfDayHours.Count()} time of day hours.");
            foreach (var t in timesOfDayHours)
            {
                var timeOfDayForecast = new NwsTimeOfDayForecast();

                timeOfDayForecast.StartTime = t.FirstOrDefault().StartTime;
                timeOfDayForecast.EndTime = t.LastOrDefault().EndTime;

                var times = timeOfDayForecast.StartTime.Hour switch
                {
                    >= 0 and < 6 => ("Early Morning", "PeriodEarlyMorning", "time_morning.json"),
                    >= 6 and < 12 => ("Morning", "PeriodMorning", "time_late_morning.json"),
                    >= 12 and < 18 => ("Afternoon", "PeriodAfternoon", "time_afternoon.json"),
                    >= 18 and < 22 => ("Evening", "PeriodEvening", "time_evening.json"),
                    _ => ("Night", "PeriodNight", "time_night.json")
                };
                // var todEvents = new ObservableCollection<EventTime>();
                //todEvents = _session.EventTimes.Where(x => x.WeatherHour.Date == date && x.TimeOfDay == times.Item1).ToObservableCollection();
                timeOfDayForecast.TimeOfDay = times.Item1;

                timeOfDayForecast.TimeOfDayGradientBrush = (LinearGradientBrush)Application.Current.Resources[times.Item2];
                var animation = new SKFileLottieImageSource { File = times.Item3 };
                var weatherAnimations = t.Select(x => new LottieAnimation
                {
                    Temperature = x.TempAndUnit,
                    Time = x.StartTime,
                    Forecast = x.ShortForecast,
                    LottieWeather = x.SkHourlyAnimation
                });
                timeOfDayForecast.SkLottieCollection = new ObservableCollection<LottieAnimation>(weatherAnimations);
                timeOfDayForecast.SKTimeOfDayAnimation = animation;
                timeOfDayForecast.Forecasts = hourlies.ToArray();
                timeOfDayForecast.TimeOfDayEvents = new ObservableCollection<EventTime>();


                timeOfDayForecast.TimeOfDayForecast = await BuildForecastAsync(hourlies);
                timeOfDayCollection.Add(timeOfDayForecast);

            }

            var timeOfDay = timeOfDayCollection.Where(y => y.StartTime >= _session.NwsData.DailyForecasts[i].StartTime && _session.NwsData.DailyForecasts[i].EndTime <= y.EndTime).OrderBy(x => x.StartTime).ToArray();
            _session.NwsData.DailyForecasts[i].TimeOfDayForecasts = timeOfDay.ToArray();
        }
        var masterTimeOfDayArray = timeOfDayCollection.OrderBy(o => o.StartTime).ToArray();
        _session.NwsData.TimeOfDayForecasts = masterTimeOfDayArray.ToArray();
        return masterTimeOfDayArray;
    }

    public async Task<string> BuildForecastAsync(NwsHourlyForecast[] forecasts)
    {

        ObservableCollection<(DateTime starttime, DateTime endtime, string line)> starterCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string line)>();
        ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double temp)> tempCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double temp)>();
        ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double windspeed, string direction)> windCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double windspeed, string direction)>();
        ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double gust)> gustsCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double gust)>();
        ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double skycover)> skyCoverCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double skycover)>();
        ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double tstorm)> tStormCollection = new ObservableCollection<(DateTime starttime, DateTime endtime, string trend, double tstorm)>();
        //ShortForecast
        for (int f = 0; f < forecasts.Length - 1; f++)
        {
            DateTime starttime = forecasts[f].StartTime;
            DateTime endtime = forecasts[f].EndTime;
            string line = forecasts[f].ShortForecast;
            double temp = Math.Round(forecasts[f].Temperature.Value / 5) * 5;
            double windSpeed = Math.Round(forecasts[f].WindSpeed.Value / 5) * 5;
            double futureTemp = Math.Round(forecasts[f + 1].Temperature.Value / 5) * 5;
            double futureWindSpeed = Math.Round(forecasts[f + 1].WindSpeed.Value / 5) * 5;
            string direction = forecasts[f].WindDirection;
            double gust = forecasts[f].WindGust != null ? Math.Round(forecasts[f].WindGust.Value / 5) * 5 : 0;
            double skyCover = forecasts[f].Supplemental.Skycover != null ? forecasts[f].Supplemental.Skycover.Value : -1;
            double tStorm = forecasts[f].Supplemental.ProbabilityOfThunder != null && forecasts[f].Supplemental.ProbabilityOfThunder.Value >= 10 ? forecasts[f].Supplemental.ProbabilityOfThunder.Value : -1;
            double futureGust = forecasts[f + 1].WindGust != null ? Math.Round(forecasts[f + 1].WindGust.Value / 5) * 5 : 0;
            double futureSkyCover = forecasts[f + 1].Supplemental.Skycover != null ? forecasts[f + 1].Supplemental.Skycover.Value : -1;
            double futureTStorm = forecasts[f + 1].Supplemental.ProbabilityOfThunder != null && forecasts[f + 1].Supplemental.ProbabilityOfThunder.Value >= 10 ? forecasts[f + 1].Supplemental.ProbabilityOfThunder.Value : -1;
            starterCollection.Add((starttime, endtime, line));
            tempCollection.Add((starttime, endtime, ProcessTrend(temp, futureTemp, starttime, endtime), temp));
            windCollection.Add((starttime, endtime, ProcessTrend(windSpeed, futureWindSpeed, starttime, endtime), windSpeed, direction));
            gustsCollection.Add((starttime, endtime, ProcessTrend(gust, futureGust, starttime, endtime), gust));
            skyCoverCollection.Add((starttime, endtime, ProcessTrend(skyCover, futureSkyCover, starttime, endtime), skyCover));
            tStormCollection.Add((starttime, endtime, ProcessTrend(tStorm, futureTStorm, starttime, endtime), tStorm));
        }
        //Starter
        var orderStarters = starterCollection.OrderBy(x => x.starttime).GroupBy(y => y.line);
        var phrases = orderStarters.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} {x.Key}").ToObservableCollection();
        var firstLine = String.Join(", ", phrases) + ". ";
        //Temperature
        var orderTemps = tempCollection.OrderBy(x => x.starttime).GroupBy(y => y.temp);
        var tempPhrases = orderTemps.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} at a temperature of {x.Key} and {x.Max(y => y.trend)}").ToObservableCollection();
        var tempLine = String.Join(", ", tempPhrases) + ". ";
        //WindSpeed and Direction
        var orderWinds = windCollection.OrderBy(x => x.starttime).GroupBy(y => y.windspeed);
        var windsPhrases = orderWinds.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} {x.Max().direction} winds at speeds of {Math.Round(x.Key / 5) * 5} mph and {x.Max(y => y.trend)}").ToObservableCollection();
        var windLine = String.Join(", ", windsPhrases) + ". ";
        //Gusts
        var orderGusts = gustsCollection.OrderBy(x => x.starttime).GroupBy(y => y.gust);
        var gustPhrases = orderGusts.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} gusts up to {x.Key} mph and {x.Max(y => y.trend)}").ToObservableCollection();
        var gustLine = String.Join(", ", gustPhrases) + ". ";
        //Skycover
        var orderClouds = skyCoverCollection.OrderBy(x => x.starttime).GroupBy(y => y.skycover);
        var cloudPhrases = orderClouds.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} cloud coverage is {x.Key}% and {x.Max(y => y.trend)}").ToObservableCollection();
        var cloudLine = String.Join(", ", cloudPhrases) + ". ";
        //TStorms
        var orderTstorms = tStormCollection.Where(r => r.tstorm != -1).OrderBy(x => x.starttime).GroupBy(y => y.tstorm);
        var tstormPhrases = orderTstorms.Select(x => $"from {x.Min(z => z.starttime).ToString("h tt")} to {x.Max(y => y.endtime).ToString("h tt")} there is a {x.Key}% chance of thunderstorms and damaging winds and {x.Max(y => y.trend)}").ToObservableCollection();
        var tstormLine = String.Join(", ", tstormPhrases) + ". ";
        string complete = firstLine + tstormLine + tempLine + cloudLine + windLine + gustLine;
        return complete;
    }




    public string ProcessTrend(double oldValue, double newValue, DateTime oldTime, DateTime newTime)
    {
        try
        {
            var rate = Math.Abs(newValue - oldValue) / Math.Abs(newTime.Hour - oldTime.Hour);
            if (rate < _session.NwsData.HourlyTemperatureChangeRate && oldValue < newValue)
            {
                return "slow increase";
            }
            else if (rate > _session.NwsData.HourlyTemperatureChangeRate && oldValue < newValue && Math.Abs(Math.Abs(oldValue) - Math.Abs(newValue)) >= 10)
            {
                return "rapid increase";
            }
            else if (rate < _session.NwsData.HourlyTemperatureChangeRate && oldValue > newValue && Math.Abs(Math.Abs(oldValue) - Math.Abs(newValue)) >= 10)
            {
                return "slow decrease";
            }
            else if (rate > _session.NwsData.HourlyTemperatureChangeRate && oldValue > newValue && Math.Abs(Math.Abs(oldValue) - Math.Abs(newValue)) >= 10)
            {
                return "rapid decrease";
            }
            else
            {
                return "remains steady";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR PROCESSING TREND{ex}");
            return "remains steady";
        }
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

    private static string TemperatureStatement(double t, double d)
    {
        var tempBuilder = new StringBuilder();

        string temp = string.Empty;
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
        tempBuilder.Append(temp);
        return tempBuilder.ToString();
    }
    private string DewPointStatement(double d, double t)
    {
        var humiditySB = new StringBuilder();
        if (t >= 60)
        {
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
            humiditySB.Append($" and dew point {dp}.");
        }
        return humiditySB.ToString();
    }
    private string TypeOfDay(double t, double d)
    {
        string feelsLike = string.Empty;
        var typeOfDaySb = new StringBuilder();
        var coldness = string.Empty;
        switch (t)
        {
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
                feelsLike = "wicked cold";
                break;
            case (> 30 and <= 50):
                feelsLike = "perfect";
                break;
            case (> 50 and <= 60) when t >= 60 && t < 65:
                feelsLike = "decent";
                break;
            case (> 50 and <= 60) when t > 65:
                feelsLike = "kinda comfortable";
                break;
            case (> 60 and <= 70) when t >= 70 && t <= 75:
                feelsLike = "sweaty";
                break;
            case (> 60 and <= 70) when t > 75:
                feelsLike = "kinda  comfortable";
                break;
            case > 70 and <= 75:
                feelsLike = "filthy, hot, and sweaty";
                break;
            case > 75:
                //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
                feelsLike = "wicked hot and dangerous";
                break;
        }
        typeOfDaySb.Append($", a {feelsLike}");
        return typeOfDaySb.ToString();
    }
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

    private static ForecastItem[] BuildWeatherItems(IEnumerable<NwsHourlyForecast> forecasts)
    {
        return forecasts
            .SelectMany(forecast => forecast.Supplemental?.Weather ?? [])
            .Where(weather => weather != null &&
                              !string.IsNullOrWhiteSpace(weather.Category))
            .GroupBy(
                weather => weather.Category,
                StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var start = group.Min(weather => weather.Time);
                var duration = group.FirstOrDefault().Duration;
                var end = start.AddHours(duration);


                return new ForecastItem
                {
                    Condition = group.Key,
                    StartTime = start,
                    EndTime = end == start ? start.AddHours(1) : end
                };
            })
            .OrderBy(item => item.StartTime)
            .ToArray();
    }



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



    //private string BuildStormAndWarningStatement(List<ValueEvolution> evolutions)
    //{
    //    var weatherStatements = _session.NwsData.DailyForecasts.OrderBy(o => o.StartTime).ToArray();
    //    if (weatherStatements != null)
    //    {
    //        var thunderKeywords = new[] { "thunder", "thunderstorm", "severe", "tstm" };
    //        var stormHours = weatherStatements.Where(e => e.Weather?.Statement != null)
    //                                    .OrderBy(e => e.Time)
    //                                    .ToArray();

    //        if (stormHours.Length == 0) return string.Empty;

    //        var start = stormHours.First().Time;
    //        var end = stormHours.Last().Time == default ? stormHours.Last().Time.AddHours(1) : stormHours.Last().Time;
    //        var msg = new StringBuilder();
    //        msg.Append($"⚠️ Severe weather possible from {start:h tt} to {end:h tt}. ");
    //        if (_session.NwsData.HourlySupplementals.Where(x => x.Time.Date == start.Date).Select(t => t.ProbabilityOfThunder?.Description).ToArray() is string[] tstormDescription)
    //        {
    //            //From x to x there is a moderate change of thrudersstorms.
    //            msg.Append("Prepare for a moderate chance of thunderstorms and strong winds.");
    //        }
    //        // Use the first matching statement for detail if present
    //        var detail = stormHours.Select(e => e.Weather.Statement).FirstOrDefault(s => !string.IsNullOrEmpty(s));
    //        if (!string.IsNullOrEmpty(detail)) msg.Append(detail + ". ");

    //        return msg.ToString();

    //    }
    //    return "no storms to report.";
    //}

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
        var list = evolutions.OrderBy(e => e.Time).Select(e => (Time: e.Time, desc: e.WeatherDescription?.FirstOrDefault()?.Value ?? e.WeatherStatement?.FirstOrDefault()?.Value ?? string.Empty)).ToList();            // collapse consecutive equal descriptions
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


    //public async Task<ObservableCollection<string>> BuildDailyForecastAsync(NwsTimeOfDayForecast tod)
    //{
    //    try
    //    {
    //        Debug.WriteLine("BuildDailyForecastAsync: start");

    //        var hourlies = tod.Forecasts;
    //        Debug.WriteLine($"BuildDailyForecastAsync: loaded {hourlies.Length} nws hours. StartTime={tod.StartTime:f} EndTime:{tod.EndTime:f}");

    //        // Map to ValueEvolution on background thread
    //        List<ValueEvolution> evolutions = await BuildEvolutionListsAsync(hourlies);

    //        Debug.WriteLine($"BuildDailyForecastAsync: mapped {evolutions.Count} evolutions");

    //        // Build and smooth segments on background
    //        var segments = await Task.Run(() => ForecastSegmentation.BuildSegments(evolutions));
    //        var smoothed = await Task.Run(() => ForecastSegmentation.SmoothSegments(segments, minHours: 2));

    //        Debug.WriteLine($"BuildDailyForecastAsync: built {smoothed.Count} segments");

    //        // Build statements on background, then marshal ObservableCollection creation to UI thread
    //        var statementList = await Task.Run(async () =>
    //        {
    //            var list = new List<string>();
    //            var stormStmt = await BuildTimeOfDayForecastsAsync(_session.NwsData.DailyForecasts);
    //            // if (!string.IsNullOrEmpty(stormStmt))
    //            // {
    //            //   list.Add(stormStmt);
    //            list.AddRange(BuildPrecipitationStatements(evolutions));
    //            //}
    //            var brief = BuildBriefForecastStatement(evolutions);
    //            if (!string.IsNullOrEmpty(brief)) list.Add(brief);
    //            var tempStmt = TemperatureStatement(evolutions.Max(x => x.Temperature).First(), evolutions.Max(x => x.DewPoints).First());
    //            if (!string.IsNullOrEmpty(tempStmt)) list.Add(tempStmt);
    //            var skyStmt = BuildSkyCoverStatement(evolutions);
    //            if (!string.IsNullOrEmpty(skyStmt)) list.Add(skyStmt);
    //            list.AddRange(BuildWindStatements(evolutions));
    //            if (!list.Any())
    //            {
    //                foreach (var s in smoothed)
    //                    list.Add($"{s.Start:h tt} to {s.End:h tt}: {Math.Round(s.Representative.Temperature)}°, {s.Representative.PrecipitationType}");
    //            }
    //            var forecasts = new ObservableCollection<string>(list);
    //            return forecasts;
    //        });

    //        Debug.WriteLine($"BuildDailyForecastAsync: built {statementList.Count} statements");

    //        // Marshal ObservableCollection creation to UI thread
    //        var statements = new ObservableCollection<string>(statementList);
    //        //string paragraph = String.Join(" ", statements);

    //        Debug.WriteLine("BuildDailyForecastAsync: completed successfully");
    //        return statements;

    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"BuildDailyForecastAsync: exception - {ex}");
    //        Debug.WriteLine($"Error building forecast: {ex.Message}");
    //    }

    //    return new ObservableCollection<string>();
    //}

    private async Task<List<ValueEvolution>> BuildEvolutionListsAsync(NwsHourlyForecast[] localNws)
    {
        var evolutions = await Task.Run(async () =>
        {
            var evolutions = new List<ValueEvolution>();

            //var dayHourlies = localNws.Where(h => h.StartTime.Time == localSelectedDate).OrderBy(h => h.StartTime).ToArray();
            foreach (var nws in localNws)
            {
                string precipicationType = GetPrecipitationType(nws);
                var evo = new ValueEvolution
                {
                    Time = nws.StartTime,
                    EndTime = nws.EndTime,
                    IsDay = nws.IsDaytime,
                    Temperature = new List<ForecastTemperature> { new ForecastTemperature { StartTime = nws.StartTime, Value = nws.Temperature.Value, UnitCode = nws.Temperature.UnitCode } },
                    ApparentTemperature = new List<ApparentTemperature> { new ApparentTemperature { StartTime = nws.StartTime, Value = nws.Supplemental?.ApparentTemperature?.Value ?? 0, UnitCode = nws.Supplemental.ApparentTemperature?.UnitCode ?? string.Empty } },
                    PrecipitationType = precipicationType,
                    PrecipitationAmt = GetPrecipitationType(nws) != "none" ? nws.Supplemental.QuantitativePrecipitation.Value : 0.00,
                    WindSpeed = nws.WindSpeed.Value,
                    Wind = new List<ForecastWind> { new ForecastWind { StartTime = nws.StartTime, Value = nws.WindSpeed.Value } },
                    Gusts = new List<ForecastGusts> { new ForecastGusts { StartTime = nws.StartTime, Value = nws.WindGust != null ? nws.WindGust.Value : 0.0D } },
                    SkyCover = new List<ForecastSkyCover> { new ForecastSkyCover { StartTime = nws.StartTime, Value = nws.Supplemental?.Skycover?.Value ?? 0.0D } },
                    MixingHeight = new List<ForecastMixingHeight> { new ForecastMixingHeight { StartTime = nws.StartTime, Value = nws.Supplemental?.MixingHeight?.Value ?? 0.0D } },
                    Wbgt = new List<ForecastWbgt> { new ForecastWbgt { StartTime = nws.StartTime, Value = nws.Supplemental?.WetBulbGlobeTemperature?.Value ?? 0.0D } },
                    HeatRisk = new List<ForecastHeatRisk> { new ForecastHeatRisk { StartTime = nws.StartTime, Value = nws.Supplemental?.HeatRisk?.Value ?? 0.0D } },
                    WindChill = new List<ForecastWindChill> { new ForecastWindChill { StartTime = nws.StartTime, Value = nws.Supplemental?.WindChill?.Value ?? 0.0D } },
                    WeatherStatement = new List<ForecastWeatherStatement> { new ForecastWeatherStatement { StartTime = nws.StartTime, Value = nws.ShortForecast } },
                    WeatherDescription = new List<ForecastWmoDescription> { new ForecastWmoDescription { StartTime = nws.StartTime, Value = nws.DetailedForecast } }
                };

                evolutions.Add(evo);
            }
            return evolutions;
        });
        return evolutions;
    }

    //public static string TemperatureStatement(double t,  double d)
    //{
    //    tempBuilder.Append("the temperature ");
    //    if (t < 30) temp = "below freezing";
    //    if (30 <= t && t < 34) temp = "in the low 30's";
    //    if (34 <= t && t <= 36) temp = "in the mid 30's";
    //    if (37 <= t && t < 40) temp = "in the high 30's";
    //    if (40 <= t && t < 44) temp = "in the low 40's";
    //    if (44 <= t && t <= 46) temp = "in the mid 40's";
    //    if (47 <= t && t < 50) temp = "in the high 40's";
    //    if (50 <= t && t < 54) temp = "in the low 50's";
    //    if (54 <= t && t <= 56) temp = "in the mid 50's";
    //    if (57 <= t && t < 60) temp = "in the high 50's";
    //    if (60 <= t && t < 64) temp = "in the low 60's";
    //    if (64 <= t && t <= 66) temp = "in the mid 60's";
    //    if (67 <= t && t < 70) temp = "in the high 60's";
    //    if (70 <= t && t < 74) temp = "in the low 70's";
    //    if (74 <= t && t <= 76) temp = "in the mid 70's";
    //    if (77 <= t && t < 80) temp = "in the high 70's";
    //    if (80 <= t && t < 84) temp = "in the low 80's";
    //    if (84 <= t && t <= 86) temp = "in the mid 80's";
    //    if (87 <= t && t < 90) temp = "in the high 80's";
    //    if (90 <= t && t < 94) temp = "in the low 90's";
    //    if (94 <= t && t <= 96) temp = "in the mid 90's";
    //    if (97 <= t && t < 100) temp = "in the high 90's";
    //    if (100 <= t && t < 104) temp = "in the low 100's";
    //    if (104 <= t && t <= 106) temp = "reaching about 105";
    //    if (107 <= t && t < 110) temp = "climbing near 110";
    //    if (t >= 110) temp = "reaching over 110";
    //    tempBuilder.Append(temp);

    //    var feelsLike = string.Empty;
    //    if (t >= 60)
    //    {
    //        string dp = string.Empty;
    //        if (d < 30) dp = "less than 30";
    //        if (30 <= d && d < 34) dp = "in the low 30's,";
    //        if (34 <= d && d <= 36) dp = "in the mid 30's,";
    //        if (37 <= d && d < 40) dp = "in the high 30's";
    //        if (40 <= d && d < 44) dp = "in the low 40's";
    //        if (44 <= d && d <= 46) dp = "in the mid 40's,";
    //        if (47 <= d && d < 50) dp = "in the high 40's,";
    //        if (50 <= d && d < 54) dp = "in the low 50's";
    //        if (54 <= d && d <= 56) dp = "in the mid 50's,";
    //        if (57 <= d && d < 60) dp = "in the high 50's";
    //        if (60 <= d && d < 64) dp = "in the low 60's,";
    //        if (64 <= d && d <= 66) dp = "in the mid 60's,";
    //        if (67 <= d && d < 70) dp = "in the high 60's";
    //        if (70 <= d && d < 75) dp = "in the low 70's";
    //        if (d > 75) dp = $" at {d}";
    //        tempBuilder.Append($" and dew point {dp},");


    //        var h = tempAvg;
    //        switch (hours.Max(h => h.DewPoint.Value))
    //        {
    //            case <= 50:
    //                feelsLike = "pleasant";
    //                break;
    //            case (> 50 and <= 60) when h >= 60 && h < 65:
    //                feelsLike = "mostly comfortable";
    //                break;
    //            case (> 50 and <= 60) when h > 65:
    //                feelsLike = "comfortable";
    //                break;
    //            case (> 60 and <= 70) when h >= 70 && h <= 75:
    //                feelsLike = "humid";
    //                break;
    //            case (> 60 and <= 70) when h > 75:
    //                feelsLike = "mostly comfortable";
    //                break;
    //            case > 70 and <= 75:
    //                feelsLike = "hot and humid";
    //                break;
    //            case > 75:
    //                //Dangerous when paired with high temperatures. Heat-illness risk for outdoor activity, athletes, elderly, and infants
    //                feelsLike = "tropical";
    //                break;
    //        }
    //        tempBuilder.Append($", the {TemperatureStatement(hours)} will feel {feelsLike} in the great outdoors.");

    //    }
    //    else if (tempAvg > 30 && tempAvg < 60 && hours.Max(h => h.ProbabilityOfPrecipitation.Value) <= 12.0)
    //    {
    //        tempBuilder.Append($"With a {hours.Max(h => h.ProbabilityOfPrecipitation.Value)}% chance of precipitation and cool temperatures.");
    //    }
    //    else
    //    {
    //        string coldness = string.Empty;
    //        var h = tempAvg;
    //        switch (h)
    //        {
    //            case > 20 and < 30:
    //                coldness = "cold";
    //                break;
    //            case > 10 and <= 20:
    //                coldness = "chilly";
    //                break;
    //            case <= 10:
    //                coldness = "frigid";
    //                break;
    //        }

    //        tempBuilder.Append($" will make for a {coldness} {hours.FirstOrDefault().TimeOfDay}.");
    //    }
    //    return tempBuilder.ToString();
    //}

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
        var weatherStmt = evolution.WeatherStatement?.FirstOrDefault()?.Value ?? string.Empty;

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


