using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class ForecastViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsWorking { get; set; } = false;
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> Dailys { get; set; } = new ObservableCollection<WeatherForecast>();
        [ObservableProperty]
        public partial ImageSource SelectedDailyIcon { get; set; }

        [ObservableProperty]
        public partial string SelectedDailyName { get; set; }

        [ObservableProperty]
        public partial string SelectedDailyWeatherSummary { get; set; }

        [ObservableProperty]
        public partial string SelectedDailySafetySummary { get; set; }
        [ObservableProperty]
        public partial string SelectedDailyMaxTemp { get; set; }
        [ObservableProperty]
        public partial string SelectedDailyMinTemp { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<EventTime> SelectedDailyEvents { get; set; }
        [ObservableProperty]
        public partial DateTime[] Dates { get; set; }
        [ObservableProperty]
        public partial string TimePeriod { get; set; }

        [ObservableProperty]
        public partial System.Timers.Timer _timer { get; set; }
        [ObservableProperty]
        public partial string CurrentTime { get; set; }

        private DailyForecast _selectedDaily;
        public DailyForecast SelectedDaily
        {
            get => _selectedDaily;
            private set => SetProperty(ref _selectedDaily, value);
        }

        //[ObservableProperty]
        //public partial HourlyForecast SelectedEventHourlyWeather { get; set; }

        //    public IReadOnlyList<TimeOfDay> Periods { get; } =
        //    [
        //        TimeOfDay.Overnight,
        //TimeOfDay.EarlyMorning,
        //TimeOfDay.Morning,
        //TimeOfDay.Afternoon,
        //TimeOfDay.Evening,
        //TimeOfDay.Night
        //    ];


        //    public ObservableCollection<DayPeriod> DayPeriods { get; } = new ObservableCollection<DayPeriod>
        //{
        //    new DayPeriod("Overnight"),
        //    new DayPeriod("Early Morning"),
        //    new DayPeriod("Morning"),
        //    new DayPeriod("Afternoon"),
        //    new DayPeriod("Evening"),
        //    new DayPeriod("Night")
        //};

        //    // Dictionary mapping period name to list of astronomical events
        //    private Dictionary<string, ObservableCollection<EventTime>> _eventsByPeriod = new Dictionary<string, ObservableCollection<EventTime>>();
        //    public Dictionary<string, ObservableCollection<EventTime>> EventsByPeriod
        //    {
        //        get => _eventsByPeriod;
        //        private set
        //        {
        //            _eventsByPeriod = value;
        //            OnPropertyChanged(nameof(EventsByPeriod));
        //        }
        //    }

        //    // Gradient brush representing the sky color
        //    private LinearGradientBrush _skyGradient = new LinearGradientBrush();
        //    public LinearGradientBrush SkyGradient
        //    {
        //        get => field;
        //        private set
        //        {
        //            field = value;
        //            OnPropertyChanged(nameof(SkyGradient));
        //        }
        //    } = new LinearGradientBrush();

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    UpdateEventsAndGradient().GetAwaiter().GetResult();
                }
            }
        }

        public Session Session { get; }
        private SkyColorEngine _skyColor;

        public ForecastViewModel(Session session)
        {
            Session = session;

            try
            {
                IsWorking = true;
                Dailys = new ObservableCollection<WeatherForecast>(Session.WeatherForecasts);
                Debug.WriteLine($"[ForecastVM] Initializing with SelectedLocation: {session.SelectedLocation?.DisplayName}");



                Debug.WriteLine($"[ForecastVM] DailyStartTime count: {Session.Dates.Length}");

                // SelectedDate initialization - guard against empty dates
                Dates = Session.Dates;
                SelectedDate = Session.Dates.First();
                _selectedDate = SelectedDate;
                SelectedDaily = Dailys.Where(x => new DateTime(x.Date, TimeOnly.MinValue).Date == SelectedDate).Select(d => d.DayForecast).First() ?? new DailyForecast();
                _selectedDaily = SelectedDaily;
                Debug.WriteLine($"[ForecastVM] SelectedDate set to: {SelectedDate:F}");
                TimePeriod = SelectedDaily.TimePeriod;
                SelectedDailyEvents = SelectedDaily.DailyEvents.EventTimes;
                SelectedDailyIcon = SelectedDaily.DailyIcon;
                SelectedDailyMaxTemp = $"{SelectedDaily.Temperature2mMax.Value}{SelectedDaily.Temperature2mMax.Unit}";
                SelectedDailyMinTemp = $"{SelectedDaily.Temperature2mMin.Value}{SelectedDaily.Temperature2mMin.Unit}";
                SelectedDailyName = SelectedDaily.TimePeriod;
                SelectedDailySafetySummary = SelectedDaily.HealthSafetySummary;
                SelectedDailyWeatherSummary = SelectedDaily.WeatherSummary;
                // Initialize the dictionary with empty collections
                //foreach (var period in DayPeriods)
                //{
                //    var coll = new ObservableCollection<EventTime>();
                //    EventsByPeriod[period.Name] = coll;
                //    period.Events = coll;
                //}

                // Set default date
                //SelectedDate = DateTime.Today;

                //    Debug.WriteLine($"[ForecastVM] SelectedDaily set to: {SelectedDaily?.StartTime:F}");
                //}
                //else
                //{
                //    Debug.WriteLine("[ForecastVM] WARNING: Dailys collection is empty! Creating default Daily.");
                //    SelectedDaily = new Daily();
                //}

                // SelectedDailyEvents initialization

                Debug.WriteLine($"[ForecastVM] SelectedDailyEvents.Count == SelectedDailyEvents.Count ?? 0}}");



                Debug.WriteLine($"[ForecastVM] SelectedLocation: {Session.SelectedLocation.DisplayName}");

                CurrentTime = DateTime.UtcNow.AddHours(Session.SelectedLocation.TimeZoneOffset).ToString("F");
                Debug.WriteLine($"[ForecastVM] CurrentTime: {CurrentTime}");

                // Initialize timer
                _timer = new System.Timers.Timer(1000); // 1 second interval
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();

                IsWorking = false;
                Debug.WriteLine("[ForecastVM] Initialization completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ForecastVM] ERROR in constructor: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[ForecastVM] StackTrace: {ex.StackTrace}");
                IsWorking = false;
                throw; // Re-throw so the error propagates and we can see it
            }
        }

        // inside UpdateEventsAndGradient() after SelectedDaily/... assignments, call:
        BuildPerPeriodBrushesFromSamples();

        // add this helper method to the class:
        private void BuildPerPeriodBrushesFromSamples()
        {
            // representative hour for each named period (local wall-clock hours)
            var map = new Dictionary<string, int>
            {
                ["Overnight"] = 0,
                ["Early Morning"] = 3,
                ["Morning"] = 6,
                ["Afternoon"] = 12,
                ["Evening"] = 18,
                ["Night"] = 23
            };

            // If you store DayPeriod on the VM use DayPeriods; otherwise use Session.Periods
            var periods = (this.GetType().GetProperty("DayPeriods") != null)
                ? (IEnumerable<dynamic>)this.GetType().GetProperty("DayPeriods")!.GetValue(this)
                : (IEnumerable<dynamic>)Session.Periods;

            foreach (var period in periods)
            {
                if (!map.TryGetValue(period.Name, out int hour)) continue;

                var sampleTime = SelectedDate.Date.AddHours(hour);

                // Find nearest SkyColor entry in session (assumes SkyColor.Time is DateTime)
                var nearest = Session.SkyColors
                    .OrderBy(s => Math.Abs((s.Time - sampleTime).TotalMinutes))
                    .FirstOrDefault();

                if (nearest == null)
                {
                    // fallback brush
                    period.BackgroundBrush = CreateSimpleBrush("#001122", "#021024");
                    continue;
                }

                // Use estimator with the hourly inputs already present on the SkyColor instance.
                // The estimator signature: Color EstimateSkyColor(double irradianceWm2, DateTime time, double cloudCoverPercent, double sunAltitudeDeg, double sunAzimuthDeg, DateTime sunrise, DateTime sunset, DateTime solarNoon)
                var mauiColor = _skyColor.EstimateSkyColor(
                    nearest.DirectNormalIrradiance,
                    nearest.Time,
                    nearest.CloudCoverPercent,
                    nearest.SunAltitudeDeg,
                    nearest.SunAzimuthDeg,
                    nearest.Sunrise,
                    nearest.Sunset,
                    nearest.SolarNoon);

                // Make a two-stop vertical gradient: slightly darker edge -> sampled color
                var darker = mauiColor.WithLuminosity(Math.Max(0, (float)(mauiColor.L - 0.25)));
                var stops = new GradientStopCollection
        {
            new GradientStop(darker, 0f),
            new GradientStop(mauiColor, 1f)
        };
                period.BackgroundBrush = new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
            }
        }

        // helper to build a simple 2-stop brush
        private static LinearGradientBrush CreateSimpleBrush(string startHex, string endHex)
        {
            var stops = new GradientStopCollection
    {
        new GradientStop(Color.FromArgb(startHex), 0f),
        new GradientStop(Color.FromArgb(endHex), 1f)
    };
            return new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
        }
        private async Task UpdateEventsAndGradient()
        {
            //// Clear existing events
            //foreach (var period in DayPeriods)
            //{
            //    EventsByPeriod[period.Name].Clear();
            //}

            //// Example: Populate events based on SelectedDate
            //// Replace this with real astronomical calculations or data fetch

            //EventsByPeriod["Overnight"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate && d.WeatherHour < SelectedDate.AddHours(3)).OrderBy(x => x.OrderTime));
            //EventsByPeriod["Early Morning"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate.AddHours(3) && d.WeatherHour < SelectedDate.AddHours(6)).OrderBy(x => x.OrderTime));
            //EventsByPeriod["Morning"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate.AddHours(6) && d.WeatherHour < SelectedDate.AddHours(12)).OrderBy(x => x.OrderTime));
            //EventsByPeriod["Afternoon"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate.AddHours(12) && d.WeatherHour < SelectedDate.AddHours(18)).OrderBy(x => x.OrderTime));
            //EventsByPeriod["Evening"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate.AddHours(18) && d.WeatherHour < SelectedDate.AddHours(22)).OrderBy(x => x.OrderTime));
            //EventsByPeriod["Night"] = new ObservableCollection<EventTime>(Session.EventTimes.Where(d => d.WeatherHour >= SelectedDate.AddHours(22) && d.WeatherHour < SelectedDate.AddHours(24)).OrderBy(x => x.OrderTime));
            //// Also update DayPeriod.Events references so XAML can bind directly to the period's Events
            //foreach (var period in DayPeriods)
            //{
            //    if (EventsByPeriod.TryGetValue(period.Name, out var list))
            //    {
            //        period.Events = list;
            //    }
            //    else
            //    {
            //        period.Events = new ObservableCollection<EventTime>();
            //    }
            //}
            SelectedDaily = Dailys.Where(x => new DateTime(x.Date, TimeOnly.MinValue) == SelectedDate.Date).First().DayForecast;
            SelectedDailyEvents = SelectedDaily.DailyEvents.EventTimes;
            SelectedDailyMaxTemp = $"{SelectedDaily.Temperature2mMax.Value}{SelectedDaily.Temperature2mMax.Unit}";
            SelectedDailyMinTemp = $"{SelectedDaily.Temperature2mMin.Value}{SelectedDaily.Temperature2mMin.Unit}";
            SelectedDailyIcon = SelectedDaily.DailyIcon;
            SelectedDailyName = SelectedDaily.TimePeriod;
            SelectedDailySafetySummary = SelectedDaily.HealthSafetySummary;
            SelectedDailyWeatherSummary = SelectedDaily.WeatherSummary;


            //// Get the daily gradients from SkyColorEngine
            //var gradientsDict = await _skyColor.GetDailySkyColorGradientsAsync();

            //if (gradientsDict.TryGetValue(DateOnly.FromDateTime(SelectedDate), out var gradientColors) && gradientColors != null && gradientColors.Length > 0)
            //{
            //    SkyGradient = ConvertToLinearGradientBrush(gradientColors);

            //    // Map the daily gradient to per-period brushes — pick representative stops per period
            //    try
            //    {
            //        foreach (var period in DayPeriods)
            //        {
            //            SKColor[] periodColors;
            //            switch (period.Name)
            //            {
            //                case "Overnight":
            //                    periodColors = new SKColor[] { gradientColors[0], gradientColors[1] };
            //                    break;
            //                case "Early Morning":
            //                    periodColors = new SKColor[] { gradientColors[1], gradientColors[2] };
            //                    break;
            //                case "Morning":
            //                    periodColors = new SKColor[] { gradientColors[2], gradientColors[2] };
            //                    break;
            //                case "Afternoon":
            //                    periodColors = new SKColor[] { gradientColors[2], gradientColors[3] };
            //                    break;
            //                case "Evening":
            //                    periodColors = new SKColor[] { gradientColors[3], gradientColors[0] };
            //                    break;
            //                case "Night":
            //                    periodColors = new SKColor[] { gradientColors[0], gradientColors[0] };
            //                    break;
            //                default:
            //                    periodColors = gradientColors;
            //                    break;
            //            }

            //            period.BackgroundBrush = ConvertToLinearGradientBrush(periodColors);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine($"[ForecastVM] Error mapping per-period brushes: {ex.Message}");
            //    }
            //}
            //else
            //{
            //    // Fallback gradient if no data
            //    SkyGradient = CreateFallbackGradient();
            //}
        }

        //private LinearGradientBrush CreateFallbackGradient()
        //{
        //    var date = SelectedDate.Add(DateTime.UtcNow.AddHours(Session.SelectedLocation.TimeZoneOffset).Subtract(SelectedDate));
        //    // Simple example gradient based on time
        //    var colors = new Color[3];
        //    var hour = date.Hour;

        //    if (hour >= 6 && hour < 18)
        //    {
        //        // Daytime gradient
        //        colors[0] = Colors.LightSkyBlue;
        //        colors[1] = Colors.SkyBlue;
        //        colors[2] = Colors.DeepSkyBlue;
        //    }
        //    else
        //    {
        //        // Nighttime gradient
        //        colors[0] = Colors.MidnightBlue;
        //        colors[1] = Colors.DarkBlue;
        //        colors[2] = Colors.Black;
        //    }

        //    var stops = new GradientStopCollection();
        //    int count = colors.Length;
        //    for (int i = 0; i < count; i++)
        //    {
        //        var skColor = colors[i];
        //        var mauiColor = Color.FromRgb(skColor.Red, skColor.Green, skColor.Blue);
        //        stops.Add(new GradientStop(mauiColor, (float)i / (count - 1)));
        //    }

        //    return new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
        //}


        //private LinearGradientBrush ConvertToLinearGradientBrush(SKColor[] colors)
        //{
        //    if (colors == null || colors.Length == 0)
        //        return CreateFallbackGradient();

        //    var stops = new GradientStopCollection();
        //    int count = colors.Length;
        //    for (int i = 0; i < count; i++)
        //    {
        //        var skColor = colors[i];
        //        var mauiColor = Color.FromRgb(skColor.Red, skColor.Green, skColor.Blue);
        //        stops.Add(new GradientStop(mauiColor, (float)i / (count - 1)));
        //    }

        //    return new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
        //}

        //private string GetTimePeriodTitle(Daily daily)
        //{
        //    string timePeriod = string.Empty;
        //    if (daily.IsDay)
        //    {
        //        timePeriod = $"{daily.StartTime.ToString("dddd, MMMM dd, yyyy")}{Environment.NewLine}during the day";
        //    }
        //    else
        //    {
        //        timePeriod = $"{daily.StartTime.ToString("dddd, MMMM dd, yyyy")} - {daily.EndTime.ToString("dddd, MMMM dd, yyyy")}{Environment.NewLine}overnight";
        //    }
        //    return timePeriod;
        //}
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.UtcNow.AddHours(Session.SelectedLocation.TimeZoneOffset).ToString("F");
        }

        //private async Task OnSelectedDateChangedAsync(DateTime value)
        //{

        //    SelectedDaily = Dailys.Where(x => x.StartTime == value).First() ?? Dailys.First();
        //    SelectedDailyEvents = SelectedDaily.DailyEvents;
        //    SelectedDailyCurrentEvent = SelectedDailyEvents[0];
        //    SelectedEventHourlyWeather = _ss.HourlyCollection.Where(h => SelectedDailyCurrentEvent.WeatherHour == h.StartTime).First() ?? new Hourly();
        //    return;
        //}
    }
}
