using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.Services.Modules
{
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

        public Session SharedSession { get; set; }

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
        public ForecastService(Session sharedSession)
        {

            SharedSession = sharedSession;

        }
        // --- Rich statement builders ---
        private string BuildStormStatement(List<ValueEvolution> evolutions, NwsDailyForecast nwsDaily)
        {


            var weatherStatement = new StringBuilder();
            var precipitationType = string.Empty;
            string thunderStmt = string.Empty;
            var time = evolutions.First().Time;
            var lastTime = evolutions.Last().Time;
            var hourlies = nwsDaily.HourlyWeathers;
            if (hourlies.All(s => (s.Supplemental != null && s.Supplemental?.Weather != null && s.Supplemental.QuantitativePrecipitation?.Value > 0) || s.Supplemental?.ProbabilityOfThunder?.Value >= 10))
            {
                var hourlyWeathers = hourlies.OrderBy(y => y.StartTime).Select(s => new { s.Supplemental.Weather, s.Supplemental.QuantitativePrecipitation, s.Supplemental?.ProbabilityOfThunder }).ToArray();
                var groupedWeatherEvents = hourlyWeathers.OrderBy(x => x.Weather.Time).GroupBy(i => i.Weather);

                if (groupedWeatherEvents.Any())
                {
                    foreach (var group in groupedWeatherEvents)
                    {
                        var start = group.MinBy(x => x.Weather.Time);
                        var end = group.MaxBy(x => x.Weather.Time);
                        var tsStart = group.MinBy(x => x.ProbabilityOfThunder.Time);
                        var tsEnd = group.MaxBy(x => x.ProbabilityOfThunder.Time);
                        var thunder = group.MaxBy(x => x.ProbabilityOfThunder.Value);

                        precipitationType = group.Key.Weather;

                        if (thunder.ProbabilityOfThunder?.Value >= 10)
                        {
                            string tsBegin = tsStart.ProbabilityOfThunder.Time.ToString("h tt");
                            var tsDone = tsStart.ProbabilityOfThunder.Time.Date == tsEnd.ProbabilityOfThunder.Time.Date ? $"until {tsEnd.ProbabilityOfThunder.Time.ToString("h tt")}" : $"and continuing through the night until {tsEnd.ProbabilityOfThunder.Time.ToString("h tt")},";
                            thunderStmt = $"Along with the {thunder.Weather.Weather} starting at {tsBegin} {tsDone}{thunder.ProbabilityOfThunder.description}";
                        }
                        var highestIntensity = group.MaxBy(x => x.QuantitativePrecipitation?.Value);

                        var lowestIntensity = group.MinBy(x => x.QuantitativePrecipitation?.Value);
                        string startTime = start.Weather.Time.ToString("h tt");
                        string endTime = start.Weather.Time.Date == end.Weather.Time.Date ? $"until {end.Weather.Time.ToString("h tt")}." : $"and continuing through the night until {end.Weather.Time.ToString("h tt")}.";
                        weatherStatement.Append($"HEAD'S UP! THERE'S SOME WEATHER AFOOT! Expect {start.Weather.Weather} starting at {startTime} {endTime}");
                        weatherStatement.Append(thunderStmt);
                        weatherStatement.Append($"The {start.Weather.Weather} will start off {start.Weather.Intensity} and ");
                        string startCoverage = start.Weather.Coverage;

                        if (start.QuantitativePrecipitation.Value < highestIntensity.QuantitativePrecipitation.Value && start.Weather.Intensity != highestIntensity.Weather.Intensity)
                        {
                            int highestCount = highestIntensity.Weather.Intensity.Count();
                            weatherStatement.Append($" reach a pinnacle around {highestIntensity.Weather.Time.ToString("h tt")}. ");
                            if (highestCount > 1)
                            {
                                weatherStatement.Append($"The {start.Weather.Weather} will continue at a {highestIntensity.Weather.Intensity} rate for {highestCount} hours. ");
                            }
                        }

                        var startTimeOfDay = TimeOfDayService.CalcTimeOfDay(start.Weather.Time.Hour);
                        var startDayOfWeek = start.Weather.Time.DayOfWeek;
                        var endTimeOfDay = TimeOfDayService.CalcTimeOfDay(end.Weather.Time.Hour);
                        var endDayOfWeek = end.Weather.Time.DayOfWeek;
                        int hourlyCount = lastTime.Hour - time.Hour;
                        int weatherCount = hourlyWeathers.Length;
                        if (weatherCount >= hourlyCount / 2 && evolutions[0].IsDay == true)
                        {

                            weatherStatement.Append($" The storm will stick around for most of your {startDayOfWeek} {startTimeOfDay} and can continue into the {endTimeOfDay}");
                        }
                        else if (weatherCount >= hourlyCount / 2 && start.Weather.Time.Date != end.Weather.Time.Date && evolutions[0].IsDay == false)
                        {
                            weatherStatement.Append($" The storm will stick around until your {startDayOfWeek} {startTimeOfDay} and continue until {endDayOfWeek} {endTimeOfDay}.");
                        }
                    }
                }

            }
            else
            {
                if (String.IsNullOrEmpty(weatherStatement.ToString()))
                {
                    var conditions = hourlies.OrderBy(o => o.StartTime).ToArray();
                    var grouped = conditions.GroupBy(x => x.ShortForecast);
                    List<NwsTimeOfDayForecast> timeOfDayForecasts = new();
                    foreach (var weather in grouped)
                    {
                        var c = new NwsTimeOfDayForecast();
                        c.Condition = weather.Key;
                        c.StartTime = weather.Min(x => x.StartTime);
                        c.EndTime = weather.Max(x => x.EndTime);
                        timeOfDayForecasts.Add(c);

                    }
                    var noStorm = new StringBuilder();

                    var orderedConditions = timeOfDayForecasts.OrderBy(f => f.StartTime).ToArray();

                    for (int i = 0; i < orderedConditions.Length; i++)
                    {
                        if (i == 0)
                        {
                            noStorm.Append($"During the {orderedConditions[i].TimeOfDay}, expect {orderedConditions[i].Condition} conditions from {orderedConditions[i].StartTime:h tt} until {orderedConditions[i].EndTime:h tt}.");
                        }
                        if (i >= 1)
                        {
                            noStorm.Append($"From {orderedConditions[i].StartTime:h tt} to {orderedConditions[i].EndTime:h tt}, {orderedConditions[i].Condition}.");

                        }
                        weatherStatement.Append($"{noStorm}");
                    }

                    return weatherStatement.ToString();
                }
            }
            return string.Empty;
        }
        //private string BuildStormAndWarningStatement(List<ValueEvolution> evolutions)
        //{
        //    var weatherStatements = SharedSession.NwsData.DailyForecasts.OrderBy(o => o.StartTime).ToArray();
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
        //        if (SharedSession.NwsData.HourlySupplementals.Where(x => x.Time.Date == start.Date).Select(t => t.ProbabilityOfThunder?.description).ToArray() is string[] tstormDescription)
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
            var list = evolutions.OrderBy(e => e.Time).Select(e => (Time: e.Time, desc: e.WeatherDescription?.FirstOrDefault()?.Value ?? e.WeatherStatement?.FirstOrDefault()?.Value ?? string.Empty)).ToList();

            // collapse consecutive equal descriptions
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

        public string BuildTemperatureStatement(NwsHourlyForecast[] evolutions)
        {
            for (int i = 0; i < evolutions.Length; i++)
            {
                if (evolutions[i].IsDaytime == true)
                {
                    var hiTemps = evolutions.Where(d => d.IsDaytime == true).MaxBy(h => h.Temperature.Value);

                    // var lowTemp = evolutions.Where(d => d.IsDaytime == false).MinBy(l => l.Temperature.Value);
                    return $"High of {hiTemps.Temperature.Value}{hiTemps.Temperature.UnitCode} at {hiTemps.StartTime:h tt}.";// Low of {lowTemp.Temperature.Value}{lowTemp.Temperature.UnitCode} at {lowTemp.StartTime:h tt}.";
                }
                else
                {
                    var lowTemp = evolutions.Where(d => d.IsDaytime == false).MinBy(l => l.Temperature.Value);
                    return $"Low of {lowTemp.Temperature.Value}{lowTemp.Temperature.UnitCode} at {lowTemp.StartTime:h tt}.";
                }
            }
            return string.Empty;
        }

        private string BuildSkyCoverStatement(List<ValueEvolution> evolutions)
        {
            if (evolutions == null || !evolutions.Any()) return string.Empty;
            var first = evolutions.OrderBy(e => e.Time).First();
            var last = evolutions.OrderBy(e => e.Time).Last();
            var startSky = first.SkyCover?.FirstOrDefault()?.Value ?? 0;
            var endSky = last.SkyCover?.FirstOrDefault()?.Value ?? 0;
            if (Math.Abs(endSky - startSky) < 10) return string.Empty;
            if (endSky > startSky) return $"Clouds increasing through the day: about {(int)Math.Round(startSky, 0)}% at {first.Time:h tt} to {(int)Math.Round(endSky, 0)}% by {last.Time:h tt}.";
            return $"Skies slowly clearing: about {(int)Math.Round(startSky, 0)}% at {first.Time:h tt} to {(int)Math.Round(endSky, 0)}% by {last.Time:h tt}.";
        }

        private IEnumerable<string> BuildWindStatements(List<ValueEvolution> evolutions)
        {
            var results = new List<string>();
            if (evolutions == null || !evolutions.Any()) return results;
            // find hours with gusts above thresholds
            var gusty = evolutions.Where(e => (e.Gusts?.FirstOrDefault()?.Value ?? 0) >= 25).OrderBy(e => e.Time).ToList();
            foreach (var g in gusty)
            {
                var time = g.Time;
                var gust = g.Gusts.First().Value;
                results.Add($"Gusts up to {Math.Round(gust)} mph expected around {time:h tt}. Consider securing equipment.");
            }
            // detect sustained wind increases
            var winds = evolutions.Select(e => new { e.Time, Speed = ParseWindSpeed(e.Wind?.FirstOrDefault()?.Value ?? 0) }).OrderBy(x => x.Time).ToList();
            for (int i = 1; i < winds.Count; i++)
            {
                var prev = winds[i - 1];
                var cur = winds[i];
                if (cur.Speed - prev.Speed >= 8)
                {
                    results.Add($"Winds pick up to around {Math.Round(cur.Speed)} mph by {cur.Time:h tt}.");
                }
            }
            return results;
        }

        private double ParseWindSpeed(double Value) => Value; // placeholder for unit conversion if needed


        public async Task<ObservableCollection<string>> BuildDailyForecastAsync(NwsDailyForecast nwsDaily)
        {
            try
            {
                Debug.WriteLine("BuildDailyForecastAsync: start");

                // Load minimal session arrays on the UI thread (fast assignments)


                Dates = SharedSession.Dates.ToArray();
                //nwsHourlies = SharedSession.NwsData?.SelectedDailyHourlyForecasts ?? Array.Empty<NwsHourlyForecast>();
                var nwsHourlies = nwsDaily.HourlyWeathers ?? Array.Empty<NwsHourlyForecast>();
                var firstTime = nwsHourlies.Length > 0 ? nwsHourlies[0].StartTime : DateTime.MinValue;

                //var gfsHourlies = gfsDaily.HourlyForecasts
                //    .Where(h => h.HourlyTime >= firstTime)
                //    .OrderBy(h => h.HourlyTime)
                //    .ToArray() ?? Array.Empty<HourlyForecast>();


                //if (Dates == null || Dates.Length == 0)
                //{
                //    Debug.WriteLine("BuildDailyForecastAsync: no dates available");
                //    return new ObservableCollection<string>();
                //}

                if (SelectedDate == default) SelectedDate = Dates[0].Date;

                // Make local copies for background processing
                var localNws = nwsHourlies.ToArray() ?? Array.Empty<NwsHourlyForecast>();
                //var localGfs = gfsHourlies.ToArray() ?? Array.Empty<HourlyForecast>();
                //var localSelectedDate = SelectedDate.Time;

                Debug.WriteLine($"BuildDailyForecastAsync: loaded {localNws.Length} nws hours. StartTime={nwsDaily.StartTime:f} EndTime:{nwsDaily.EndTime:f}");

                // Map to ValueEvolution on background thread
                List<ValueEvolution> evolutions = await BuildEvolutionListsAsync(localNws);

                Debug.WriteLine($"BuildDailyForecastAsync: mapped {evolutions.Count} evolutions");

                // Build and smooth segments on background
                var segments = await Task.Run(() => ForecastSegmentation.BuildSegments(evolutions));
                var smoothed = await Task.Run(() => ForecastSegmentation.SmoothSegments(segments, minHours: 2));

                Debug.WriteLine($"BuildDailyForecastAsync: built {smoothed.Count} segments");

                // Build statements on background, then marshal ObservableCollection creation to UI thread
                var statementList = await Task.Run(() =>
                {
                    var list = new List<string>();
                    var stormStmt = BuildStormStatement(evolutions, nwsDaily);
                    if (!string.IsNullOrEmpty(stormStmt)) list.Add(stormStmt);
                    list.AddRange(BuildPrecipitationStatements(evolutions));
                    var brief = BuildBriefForecastStatement(evolutions);
                    if (!string.IsNullOrEmpty(brief)) list.Add(brief);
                    var tempStmt = BuildTemperatureStatement(localNws);
                    if (!string.IsNullOrEmpty(tempStmt)) list.Add(tempStmt);
                    var skyStmt = BuildSkyCoverStatement(evolutions);
                    if (!string.IsNullOrEmpty(skyStmt)) list.Add(skyStmt);
                    list.AddRange(BuildWindStatements(evolutions));
                    if (!list.Any())
                    {
                        foreach (var s in smoothed)
                            list.Add($"{s.Start:h tt} to {s.End:h tt}: {Math.Round(s.Representative.Temperature)}°, {s.Representative.PrecipitationType}");
                    }
                    return list;
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

        private static string GetPrecipitationType(NwsHourlyForecast nws)
        {
            var precipicationType = string.Empty;
            if (nws.Supplemental?.Weather != null)
            {
                foreach (var pt in Enum.GetValues<PrecipitationType>())
                {
                    if (pt.ToString().ToLowerInvariant() == nws.Supplemental.Weather.Weather)
                    {
                        precipicationType = nws.Supplemental.Weather.Statement;
                    }
                    else
                    {
                        precipicationType = "none";
                    }


                }
            }

            return precipicationType;
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
            sb.Append($"{temp} {TempCategory(temp, dewpoint, relativeHumidity, wbgt)} and {CalcSweatFactor(heat, dewpoint)}. ");
            sb.Append(CalcCloudCoverage(sky, evolution.IsDay));
            sb.Append(CalcWind(wind));
            sb.Append(GustSpeed(gust));
            return sb.ToString();
        }
        // Lans P. Rothfusz and described in a 1990 National Weather Service (NWS) Technical Attachment (SR 90-23)
        private string CalcSweatFactor(double apparentTemperature, double dewPoint)
        {
            // Determine the Physiological Temperature Strain Index
            double ptsi;
            string confoIndex = string.Empty;

            switch (dewPoint)
            {
                case <= 30:
                    confoIndex += "feels dry";
                    break;
                case > 40 and <= 60:
                    confoIndex += "feels decent";
                    break;
                case > 60 and <= 70:
                    confoIndex += "feels muggy";
                    break;
                case > 70:
                    confoIndex += "feels tropical";
                    break;
            }


            var unit = Preferences.Get("TemperatureUnit", "fahrenheit");
            if (unit == "fahrenheit")
            {
                ptsi = apparentTemperature * 12;
                confoIndex = ptsi switch
                {
                    < 68 => "Comfortable, no sweat!",
                    >= 68 and < 86 => "noticeable warmth. Minimal or light sweat with exertion.",
                    >= 86 and < 104 => "uncomfortable. Moderate sweat to heavy sweat if exerting.",
                    >= 104 and <= 113 => "very uncomfortable. Avoidance of exertion advised. Heavy sweat regardless of the activity.",
                    > 113 => "RISK OF HEAT STROKE! Profuse sweating, high physiological strain.",
                    _ => "Comfortable, no sweat!"
                };
            }
            else
            {
                ptsi = apparentTemperature * 6.5;
                confoIndex = ptsi switch
                {
                    < 20 => "comfortable, no sweat!",
                    >= 20 and < 30 => "noticeable warmth. Minimal or light sweat with exertion.",
                    >= 30 and < 40 => "uncomfortable. Moderate sweat to heavy sweat if exerting.",
                    >= 40 and <= 45 => "very uncomfortable. Avoidance of exertion advised. Heavy sweat regardless of the activity.",
                    > 45 => "RISK OF HEAT STROKE! Profuse sweating, high physiological strain.",
                    _ => "comfortable, no sweat!"
                };
            }
            return confoIndex;
        }

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
            // --- Extreme heat danger (WBGT-based) ---
            if (wbgt >= 90) return "extremely dangerous heat";
            if (wbgt >= 82) return "dangerous heat";
            if (wbgt >= 78) return "very hot and humid";

            // --- RelativeHumidity / dew point discomfort ---
            bool oppressiveHumidity = dewPoint >= 75 || relativeHumidity >= 85;
            bool veryHumid = dewPoint >= 70 || relativeHumidity >= 75;
            bool humid = dewPoint >= 65 || relativeHumidity >= 60;

            // --- Cold stress ---
            if (temp <= 20) return "frigid";
            if (temp <= 32) return "freezing";
            if (temp <= 40)
                return humid ? "raw and cold" : "cold";

            // --- Cool to mild ---
            if (temp <= 50)
                return humid ? "chilly and damp" : "cool";

            if (temp <= 60)
                return humid ? "cool and humid" : "mildly cool";

            // --- Mild to warm ---
            if (temp <= 70)
                return humid ? "mild but humid" : "mild";

            if (temp <= 80)
            {
                if (oppressiveHumidity) return "warm and oppressive";
                if (veryHumid) return "warm and very humid";
                if (humid) return "warm and humid";
                return "warm";
            }

            // --- Hot categories ---
            if (temp <= 90)
            {
                if (oppressiveHumidity) return "hot and oppressive";
                if (veryHumid) return "hot and very humid";
                if (humid) return "hot and humid";
                return "hot";
            }

            // --- Extreme heat (temp-based) ---
            if (temp > 90)
            {
                if (oppressiveHumidity) return "extremely hot and oppressive";
                if (veryHumid) return "extremely hot and very humid";
                return "extremely hot";
            }

            return "unknown";
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

                if (hourlyExtra?.Weather?.Statement != null)
                {
                    weather = string.Join(hourlyExtra.Weather.Statement, " ");
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

}
