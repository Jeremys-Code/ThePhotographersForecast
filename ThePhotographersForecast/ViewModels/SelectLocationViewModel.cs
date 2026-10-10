using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ThePhotographersForecast.Components;
using ThePhotographersForecast.Models;
using ThePhotographersForecast.Services;
using ThePhotographersForecast.Services.Modules;
using ThePhotographersForecast.Shared;

namespace ThePhotographersForecast.ViewModels
{
    public partial class SelectLocationViewModel : ObservableObject
    {
        private event EventHandler? DataCollectionCompleted;
        private CancellationTokenSource? _cts;
        // Use shared SharedSession store's GfsData and EventsData to avoid shadowing and ensure persistence
        [ObservableProperty]
        public partial List<PhotoLocation> Suggestions { get; set; }

        [ObservableProperty]
        public partial bool ShowSuggestions { get; set; }
        [ObservableProperty]
        public partial bool ShowForecast { get; set; }
        [ObservableProperty]
        public partial Session SharedSession { get; set; }
        [ObservableProperty]
        public partial double Longitude { get; set; }
        [ObservableProperty]
        public partial double Latitude { get; set; }
        [ObservableProperty]
        public partial bool IsWorking { get; set; }
        [ObservableProperty]
        public partial string ProgressMessage { get; set; }
        [ObservableProperty]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        public partial DateTime EndDate { get; set; }
        [ObservableProperty]
        public partial bool IsInvalidDates { get; set; }
        [ObservableProperty]
        public partial string InvalidDates { get; set; }
        [ObservableProperty]
        public partial double Progress { get; set; }
        [ObservableProperty]
        public partial bool IsComplete { get; set; }
        [ObservableProperty]
        public partial string WelcomeGreeting { get; set; }
        [ObservableProperty]
        public partial bool IsStartDateLoaded { get; set; }
        [ObservableProperty]
        public partial bool IsEndDateLoaded { get; set; }
        [ObservableProperty]
        public partial EventModel[] EventData { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<WeatherForecast> WeatherForecasts { get; set; }
        [ObservableProperty]
        public partial NwsWeatherModel NwsData { get; set; }
        [ObservableProperty]
        public partial double Completed { get; set; }
        [ObservableProperty]
        public partial double Total { get; set; }
        [ObservableProperty]
        public partial string DisplayName { get; set; }
        [ObservableProperty]
        public partial DayViewModel[] Days { get; set; }
        [ObservableProperty]
        public partial PhotoLocation SelectedLocation { get; set; }
        [ObservableProperty]
        public partial CalendarDayModel[] Dates { get; set; }
        [ObservableProperty]
        public partial FutureMoonPhase[] MoonPhases { get; set; }
        [ObservableProperty]
        public partial bool StopQuerying { get; set; }

        //public int ModelChangesCount
        //{
        //    get => field;
        //    set => SetProperty(ref field, value);
        //}

        public DateTime MinDate => DateTime.Now.Date;
        public DateTime MaxDate => DateTime.Now.AddDays(15).Date;

        //UI Input Commands
        //private IGetEventService _eventService;
        // Replaced with 
        private IGetEphemerisService _ephemerisService;
        private IGetWeatherService _weatherService;
        private IGetMoonService _moonService;
        private IGetNwsData _nwsService;
        private IForecastService _forecastService;

        public SelectLocationViewModel(IGetWeatherService weatherService, Session sharedSession, IGetEphemerisService ephemerisService, IGetMoonService moonService, IGetNwsData nwsService, IForecastService forecastService)
        {
            // 3. Properly assign the matching types
            SharedSession = sharedSession;
            _weatherService = weatherService;
            _ephemerisService = ephemerisService;
            _nwsService = nwsService;
            _moonService = moonService;
            _forecastService = forecastService;
            ShowForecast = false;
            StopQuerying = false;
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await VerifyCachePermissions();
            //});

            WelcomeGreeting = $"{TimeOfDayService.TimesOfDayGreeting(DateTime.Now.Hour)}! Wecome to ";
            IsInvalidDates = false;
            StartDate = DateTime.Now.Date;
            EndDate = DateTime.Now.AddDays(6).Date;
            IsStartDateLoaded = true;
            IsEndDateLoaded = true;
            IsComplete = false;
            Total = 5;
            Completed = 0;
        }

        //private async Task VerifyCachePermissions()
        //{
        //    // Setup Cache Directory for Animation files
        //    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        //    }
        //    string cacheFile = Path.Combine(FileSystem.Current.CacheDirectory, "meteocons");
        //    if (Directory.Exists(cacheFile))
        //        return;

        //    Directory.CreateDirectory(cacheFile);
        //    if (await FileSystem.AppPackageFileExistsAsync("meteocons/lottie.dat"))
        //    {
        //        var stream = await FileSystem.OpenAppPackageFileAsync("meteocons/lottie.dat");
        //        var read = new StreamReader(stream);
        //        List<string> files = read.ReadToEnd().Split('\n').ToList();
        //        foreach (string file in files)
        //        {
        //            Helper.GetLottieFromRawResources(cacheFile + "/" + file);
        //            Debug.WriteLine($"{cacheFile}/{file} has been added to cache.");
        //        }
        //        return;
        //    }
        //    return;

        //}
        public void OnCompletedChanged()
        {
            Completed += 1;
            if (IsWorking == true && Completed > 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Progress = Completed / 5 * 100;
                });
            }
        }

        [RelayCommand]
        partial void OnStartDateChanged(DateTime value)
        {
            if (IsStartDateLoaded == false)
                return;
            IsInvalidDates = true;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ValidateDates();
            });
        }
        [RelayCommand]
        partial void OnEndDateChanged(DateTime value)
        {
            if (IsEndDateLoaded == false)
                return;
            IsInvalidDates = true;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ValidateDates();
            });
        }

        private async Task ValidateDates()
        {
            if (StartDate > EndDate)
            {
                if (EndDate.AddDays(6).Date <= MaxDate)
                {
                    EndDate = EndDate.AddDays(6).Date;
                }
                else if (StartDate == MaxDate)
                {
                    EndDate = MaxDate;
                }
                else
                {
                    EndDate = MaxDate;
                }

            }
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                SharedSession.StartDate = StartDate;
                SharedSession.EndDate = EndDate;
            });
            IsInvalidDates = false;
        }

        [RelayCommand]
        private async Task GetGps()
        {
            Progress = 0;
            Completed = 0;
            IsWorking = true;
            StopQuerying = true;
            try
            {
                ProgressMessage = "Getting GPS location...";
                //   MainThread.BeginInvokeOnMainThread(async () =>
                //   {
                var coordinates = await CurrentLocation.VerifyPermissionAsync();
                if (coordinates.HasValue)
                {
                    Longitude = coordinates.Value.Longitude;
                    Latitude = coordinates.Value.Latitude;
                    SharedSession.SelectedLocation.Latitude = Latitude;
                    SharedSession.SelectedLocation.Longitude = Longitude;
                }
                else
                {
                    return;
                }

                Debug.WriteLine($"Latitude: {Latitude}");
                Debug.WriteLine($"Longitude: {Longitude}");
                OnCompletedChanged();
                ProgressMessage = "Setting Dates...";
                //await ValidateDates();
                int nbrOfDays = EndDate.Date.Subtract(StartDate.Date).Days + 1;
                var currentDate = StartDate.Date;
                Dates = new CalendarDayModel[nbrOfDays];
                for (int i = 0; i < nbrOfDays; i++)
                {
                    currentDate = currentDate.Date;
                    Dates[i] = new CalendarDayModel
                    {
                        Date = currentDate
                    };
                    currentDate = currentDate.Date.AddDays(1);
                }
                OnCompletedChanged();

                SharedSession.Dates = [.. Dates];
                ProgressMessage = $"Fetching forecasts...";

                var nwsResults = await _nwsService.GetWeatherAsync(Latitude, Longitude).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                SharedSession.NwsData = new NwsWeatherModel();
                SharedSession.NwsData.DailyForecasts = new NwsDailyForecast[nwsResults.DailyForecasts.Length];
                SharedSession.NwsData.HourlyForecasts = new NwsHourlyForecast[nwsResults.HourlyForecasts.Length];
                SharedSession.NwsData.HourlySupplementals = new NwsHourlySupplemental[nwsResults.HourlySupplementals.Length];
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {

                    NwsData = new NwsWeatherModel(nwsResults);
                    SharedSession.NwsData = new NwsWeatherModel(nwsResults);
                    SelectedLocation = SharedSession.SelectedLocation;
                });


                Debug.WriteLine($"_SharedSession.DailyForecasts count {SharedSession.NwsData.DailyForecasts?.Length ?? 0}");
                Debug.WriteLine($"_SharedSession.HourlySupplementals count {SharedSession.NwsData.HourlySupplementals?.Length ?? 0}");

                OnCompletedChanged();
                ProgressMessage = "Lining up the Sun, the Moon, and stars...";
                var eventResults = await _ephemerisService.GetDailyAstronomicalEventsAsync(Latitude, Longitude, NwsData, SharedSession.SelectedTimeZoneOffset).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                if (eventResults == null)
                {
                    Debug.WriteLine("Event service returned null.");
                    return;
                }
                EventData = [.. eventResults];
                SharedSession.EventsData = [.. eventResults];

                OnCompletedChanged();

                ProgressMessage = $"Calling the station in {SharedSession.SelectedLocation.City} for the weather...";

                var weatherResults = await _weatherService.BuildWeatherForecastsAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

                if (weatherResults.DailyForecasts.Length == Dates.Length)
                {
                    Debug.WriteLine($"The weather service completed successfully.");
                }
                else
                {
                    Debug.WriteLine("The weather service failed to build forecasts.");
                }

                // ProgressMessage = "Fetching moon phases...";

                await BuildSuperDuperForecastAsync();

                //if (moonResults != null)
                //{
                //   SharedSession.MoonPhases = [.. moonResults.OrderBy(o => o.UtcDateTime).ToArray()];
                //    Debug.WriteLine($"SharedSession.MoonPhases Count: {SharedSession.MoonPhases.Length}");
                //}
                //OnCompletedChanged();
                ProgressMessage = "Building event days...";
                Days = new DayViewModel[nwsResults.DailyForecasts.Length];
                if (SharedSession.NwsData.DailyForecasts.Length > 0 && SharedSession.NwsData.HourlyForecasts.Length > 0 && SharedSession.NwsData.HourlySupplementals.Length > 0 && SharedSession.EventsData.Length > 0)
                {
                    await BuildDayPeriodsAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                }


                ProgressMessage = "All Done.";
                // await EnrichForecastAsync();
                //  ProgressMessage = "Making sense of the weather...";
                //if (SharedSession.NwsData.DailyForecasts.Where(d => d.EnrichedForecast != null).Any())
                //{
                //    ProgressMessage = "All done.";
                //}
                //    });
                Debug.WriteLine($"SharedSession.WeatherForecasts Count: {SharedSession.WeatherForecasts.DailyForecasts.Length}");
                Debug.WriteLine($"SharedSession.WeatherForecasts Hours Count: {SharedSession.WeatherForecasts.HourlyForecasts.Length}");
                Debug.WriteLine($"SharedSession.Days Count: {SharedSession.Days.Length}");
                Debug.WriteLine($"SharedSession.EventModels Count: {SharedSession.EventsData.Length}");
                Debug.WriteLine($"SharedSession.NwsData.DailyForecasts Count: {SharedSession.NwsData.DailyForecasts?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.NwsData.SelectedDailyHourlyForecasts Count: {SharedSession.NwsData.HourlyForecasts?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.NwsData.HourlySupplementals Count: {SharedSession.NwsData.HourlySupplementals?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.Dates Count: {SharedSession.Dates.Length}");
                //           Debug.WriteLine($"SharedSession.MoonPhases Count: {SharedSession.MoonPhases.Length}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetGps: {ex}");
                await Shell.Current.DisplayAlertAsync("Weather Service Error", "An error occurred while retrieving forecast data. Please try again.", "OK");
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsWorking = false;
                    ShowForecast = true;
                });

            }

            //await Shell.Current.GoToAsync("ForecastDetails");
        }

        private async Task BuildSuperDuperForecastAsync()
        {
            for (int d = 0; d < SharedSession.NwsData.DailyForecasts.Length; d++)
            {
                var statements = await _forecastService.BuildDailyForecastAsync(SharedSession.NwsData.DailyForecasts[d]);
                string paragraph = String.Join(" ", statements);
                SharedSession.NwsData.DailyForecasts[d].EnrichedForecast = paragraph;
                Debug.WriteLine($"BuildSuperDuperForecastAsync: completed successfully for {SharedSession.NwsData.DailyForecasts[d].StartTime.ToString("MM/dd/yyyy")}");
            }
            return;
        }

        // safe get helper (add near top of class)
        private static string GetStormSummary(NwsHourlyForecast dh)
        {
            if (dh.Supplemental.Weather == null) return "There are no severe weather warnings for this time period.";
            var summary = dh.Supplemental.Weather.Statement;
            if (string.IsNullOrEmpty(summary))
            {
                Debug.WriteLine("Missing supplemental.Weather for hourly forecast at {Time}", dh.StartTime);
                return "There are no severe weather warnings for this time period.";
            }
            return summary;
        }

        private async Task BuildDayPeriodsAsync()
        {
            SharedSession.Periods = new ObservableCollection<DayPeriod>();
            if (Application.Current == null) return;
            SharedSession.Days = new DayViewModel[SharedSession.NwsData.DailyForecasts.Length];
            if (SharedSession.NwsData.DailyForecasts.Length > 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    for (int p = 0; p < SharedSession.NwsData.DailyForecasts.Length; p++)
                    {
                        var selectedDate = SharedSession.NwsData.DailyForecasts[p].StartTime;

                        SharedSession.Days[p] = new DayViewModel
                        {
                            Date = selectedDate,
                            Day = selectedDate.Day,
                            Events = SharedSession.EventsData.Where(e => e.Date == DateOnly.FromDateTime(selectedDate.Date)).FirstOrDefault<EventModel>() ?? new EventModel(),
                            IsEmpty = false,
                            IsEventDay = true,
                            IsFlipped = false,
                            IsToday = selectedDate.Date == DateTime.UtcNow.AddSeconds(SharedSession.SelectedTimeZoneOffset).Date,
                            SelectedLocation = new LocationViewModel
                            {
                                Location = SharedSession.SelectedLocation.DisplayName,
                                Elevation = SharedSession.SelectedLocation.Elevation,
                                TimeZone = SharedSession.SelectedTimeZone,
                                TimeZoneOffset = SharedSession.SelectedTimeZoneOffset
                            },
                            Name = SharedSession.NwsData.DailyForecasts[p].Name,
                            Periods = new ObservableCollection<DayPeriod>(),

                            DailyDetailedForecast = SharedSession.NwsData.DailyForecasts[p].DetailForecast
                        };


                        for (int f = 0; f < SharedSession.NwsData.DailyForecasts[p].HourlyWeathers.Length; f++)
                        {
                            var dh = SharedSession.NwsData.DailyForecasts[p].HourlyWeathers[f];
                            //     var od = weatherForecasts.Where(h => h.Date == dh.StartTime.Date).FirstOrDefault().DailyForecasts;
                            //     var om = od.HourlyForecasts.Where(t => t.HourlyTime == dh.StartTime).FirstOrDefault();
                            var dp = dh.StartTime.Hour switch
                            {
                                >= 0 and < 3 => new DayPeriod
                                {
                                    TimeOfDayName = $"Early Hours",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyHours"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_late_night.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                >= 3 and < 6 => new DayPeriod
                                {
                                    TimeOfDayName = $"Early Morning",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEarlyMorning"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_morning.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                >= 6 and < 12 => new DayPeriod
                                {
                                    TimeOfDayName = $"Morning",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodMorning"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_late_morning.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                >= 12 and < 18 =>
                                new DayPeriod
                                {
                                    TimeOfDayName = $"Afternoon",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodAfternoon"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_afternoon.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                >= 18 and < 22 =>
                                new DayPeriod
                                {
                                    TimeOfDayName = $"Evening",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodEvening"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_evening.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                >= 22 and < 24 =>
                                new DayPeriod
                                {
                                    TimeOfDayName = $"Night",
                                    Time = dh.StartTime,
                                    TimeOfDayBackgroundBrush = (LinearGradientBrush)Application.Current.Resources["PeriodNight"],
                                    TimeOfDayImage = Helper.GetLottieFromRawResources("time_night.json"),
                                    TimeOfDayDetailForecast = dh.DetailedForecast,
                                    TimeOfDayWeatherImages = dh.SkHourlyAnimation,
                                    TimeOfDayStormSummary = GetStormSummary(dh),
                                    TimeOfDayShortForecast = dh.ShortForecast,
                                    TimeOfDayApparentTemperature = dh.Supplemental?.ApparentTemperature != null ? $"{(int)Math.Round(dh.Supplemental.ApparentTemperature.Value, 0)}{dh.Supplemental.ApparentTemperature.UnitCode}" : "",
                                    TimeOfDayDewpoint = $"{(int)Math.Round(dh.DewPoint.Value, 0)}{dh.DewPoint.UnitCode}",
                                    TimeOfDayGust = dh.WindGust != null ? $"{(int)Math.Round(dh.WindGust.Value, 0)}{dh.WindGust.UnitCode}" : "",
                                    TimeOfDayWind = dh.WindStatement,
                                    TimeOfDayTemperature = dh.TempAndUnit
                                },
                                _ => new DayPeriod()

                            };

                            var events = SharedSession.EventTimes.Where(s => s.AstroTime.Date == dp.Time.Date).ToArray();
                            for (int e = 0; e < events.Length; e++)
                            {
                                if (events[e].TimeOfDay == dp.TimeOfDayName)
                                {
                                    dp.TimeOfDayEvents.Add(events[e]);
                                }
                            }

                            SharedSession.Periods.Add(dp);

                        }

                        Debug.WriteLine($"{SharedSession.Days[p].Date.ToString("F")} Finished and has been added to the SharedSession.Day Enumerable");
                    }

                    Debug.WriteLine($"{SharedSession.Days.Length} Enumerable SharedSession.Days done.");

                }).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Error", "DayViewModels didn't complete.", "OK");
                return;
            }

            return;
        }

        [RelayCommand]
        private async Task SelectedSuggestion(PhotoLocation parameter)
        {
            IsWorking = true;
            StopQuerying = true;
            try
            {
                if (parameter is PhotoLocation selectedLocation)
                {
                    ProgressMessage = "Setting your selected location...";
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SharedSession.SelectedLocation = new PhotoLocation(selectedLocation);
                        DisplayName = selectedLocation.DisplayName;
                        ShowSuggestions = false;

                    });
                }
                else
                {
                    return;
                }

                Debug.WriteLine($"Latitude: {Latitude}");
                Debug.WriteLine($"Longitude: {Longitude}");
                OnCompletedChanged();
                ProgressMessage = "Setting Dates...";
                //await ValidateDates();
                int nbrOfDays = EndDate.Date.Subtract(StartDate.Date).Days + 1;
                var currentDate = StartDate.Date;
                Dates = new CalendarDayModel[nbrOfDays];
                for (int i = 0; i < nbrOfDays; i++)
                {
                    currentDate = currentDate.Date;
                    Dates[i] = new CalendarDayModel
                    {
                        Date = currentDate
                    };
                    currentDate = currentDate.Date.AddDays(1);
                }
                OnCompletedChanged();

                SharedSession.Dates = [.. Dates];
                ProgressMessage = $"Fetching forecasts...";

                var nwsResults = await _nwsService.GetWeatherAsync(Latitude, Longitude).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                SharedSession.NwsData = new NwsWeatherModel();
                SharedSession.NwsData.DailyForecasts = new NwsDailyForecast[nwsResults.DailyForecasts.Length];
                SharedSession.NwsData.HourlyForecasts = new NwsHourlyForecast[nwsResults.HourlyForecasts.Length];
                SharedSession.NwsData.HourlySupplementals = new NwsHourlySupplemental[nwsResults.HourlySupplementals.Length];
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    NwsData = new NwsWeatherModel(nwsResults);
                    SharedSession.NwsData = new NwsWeatherModel(nwsResults);
                    SelectedLocation = SharedSession.SelectedLocation;
                });


                Debug.WriteLine($"_SharedSession.DailyForecasts count {SharedSession.NwsData.DailyForecasts?.Length ?? 0}");
                Debug.WriteLine($"_SharedSession.HourlySupplementals count {SharedSession.NwsData.HourlySupplementals?.Length ?? 0}");

                OnCompletedChanged();
                ProgressMessage = "Lining up the Sun, the Moon, and stars...";
                var eventResults = await _ephemerisService.GetDailyAstronomicalEventsAsync(Latitude, Longitude, NwsData, SharedSession.SelectedTimeZoneOffset).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                if (eventResults == null)
                {
                    Debug.WriteLine("Event service returned null.");
                    return;
                }
                EventData = [.. eventResults];
                SharedSession.EventsData = [.. eventResults];

                OnCompletedChanged();

                ProgressMessage = $"Calling the station in {SharedSession.SelectedLocation.City} for the weather...";

                var weatherResults = await _weatherService.BuildWeatherForecastsAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

                if (weatherResults.DailyForecasts.Length == Dates.Length)
                {
                    Debug.WriteLine($"The weather service completed successfully.");
                }
                else
                {
                    Debug.WriteLine("The weather service failed to build forecasts.");
                }

                // ProgressMessage = "Fetching moon phases...";

                await BuildSuperDuperForecastAsync();

                //if (moonResults != null)
                //{
                //   SharedSession.MoonPhases = [.. moonResults.OrderBy(o => o.UtcDateTime).ToArray()];
                //    Debug.WriteLine($"SharedSession.MoonPhases Count: {SharedSession.MoonPhases.Length}");
                //}
                //OnCompletedChanged();
                ProgressMessage = "Building event days...";
                Days = new DayViewModel[nwsResults.DailyForecasts.Length];
                if (SharedSession.NwsData.DailyForecasts.Length > 0 && SharedSession.NwsData.HourlyForecasts.Length > 0 && SharedSession.NwsData.HourlySupplementals.Length > 0 && SharedSession.EventsData.Length > 0)
                {
                    await BuildDayPeriodsAsync().ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                }


                ProgressMessage = "All Done.";
                // await EnrichForecastAsync();
                //  ProgressMessage = "Making sense of the weather...";
                //if (SharedSession.NwsData.DailyForecasts.Where(d => d.EnrichedForecast != null).Any())
                //{
                //    ProgressMessage = "All done.";
                //}
                //    });
                Debug.WriteLine($"SharedSession.WeatherForecasts Count: {SharedSession.WeatherForecasts.DailyForecasts.Length}");
                Debug.WriteLine($"SharedSession.WeatherForecasts Hours Count: {SharedSession.WeatherForecasts.HourlyForecasts.Length}");
                Debug.WriteLine($"SharedSession.Days Count: {SharedSession.Days.Length}");
                Debug.WriteLine($"SharedSession.EventModels Count: {SharedSession.EventsData.Length}");
                Debug.WriteLine($"SharedSession.NwsData.DailyForecasts Count: {SharedSession.NwsData.DailyForecasts?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.NwsData.SelectedDailyHourlyForecasts Count: {SharedSession.NwsData.HourlyForecasts?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.NwsData.HourlySupplementals Count: {SharedSession.NwsData.HourlySupplementals?.Length ?? 0}");
                Debug.WriteLine($"SharedSession.Dates Count: {SharedSession.Dates.Length}");
                //           Debug.WriteLine($"SharedSession.MoonPhases Count: {SharedSession.MoonPhases.Length}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetGps: {ex}");
                await Shell.Current.DisplayAlertAsync("Weather Service Error", "An error occurred while retrieving forecast data. Please try again.", "OK");
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsWorking = false;
                    ShowForecast = true;
                });

            }

            //await Shell.Current.GoToAsync("ForecastDetails");
        }


        //private async Task<List<string>> EnrichForecastAsync(NwsDailyForecast[] dailys)
        //{

        //    var enriched = new List<string>();

        //    for (int e = 0; e < dailys.Length; e++)
        //    {
        //        try
        //        {
        //            var enrichedForecast = await _forecastService.BuildDailyForecastAsync(dailys[e]).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        //            enriched.Add(enrichedForecast);
        //            dailys[e].EnrichedForecast = enrichedForecast;
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine("THE NEXT LINE IS WICED IMPORTANT MAKE SURE YOU FIND IT!!!!!!!!!");
        //            Debug.WriteLine($"Enrichment Failed: processing {dailys[e].StartTime.ToString("F")}");
        //        }
        //    }

        //    return enriched;
        //}

        //Part 2: Mapsui static method for turning the geographic coordinates into a layer
        //public static ILayer CreatePolygonLayer(List<(double Lon, double Lat)> coords)
        //{
        //    if (coords == null || coords.Count == 0)
        //        return new MemoryLayer { Name = "NWS Polygon", Features = new GeometryFeature[0] };

        //    // Convert lon/lat to Spherical Mercator (EPSG:3857) coordinates and build NTS Coordinates
        //    var ntsCoords = coords.Select(c =>
        //    {
        //        var merc = SphericalMercator.FromLonLat(c.Lon, c.Lat);
        //        return new NetTopologySuite.Geometries.Coordinate(merc.x, merc.y);
        //    }).ToArray();

        //    if (ntsCoords.Length < 3)
        //        return new MemoryLayer { Name = "NWS Polygon", Features = new GeometryFeature[0] };

        //    // Ensure ring is closed (first == last)
        //    if (!ntsCoords.First().Equals2D(ntsCoords.Last()))
        //    {
        //        var list = ntsCoords.ToList();
        //        list.Add(new NetTopologySuite.Geometries.Coordinate(ntsCoords.First().X, ntsCoords.First().Y));
        //        ntsCoords = list.ToArray();
        //    }

        //    var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 3857);
        //    var shell = geometryFactory.CreateLinearRing(ntsCoords);
        //    var polygon = geometryFactory.CreatePolygon(shell);

        //    var feature = new GeometryFeature { Geometry = polygon };
        //    feature.Styles.Add(new VectorStyle
        //    {
        //        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(80, 0, 120, 255)), // translucent blue
        //        Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.DarkBlue, 2)
        //    });

        //    return new MemoryLayer
        //    {
        //        Name = "NWS Polygon",
        //        Features = new[] { feature }
        //    };
        //}
        // Continue to the BackgroundMapService


        // Creating groups for hours and days
        //    private async Task CreateDailiesHourliesAsync()
        //      {
        //        return;
        //      var daily = SharedSession.WeatherForecasts.Where(w => w.Time == w.StartTime.Time).FirstOrDefault();
        //    if (daily == null) return null;
        //for (int i = 0; i < SharedSession.NwsData.SelectedDailyHourlyForecasts.Length; i++)
        //{
        //    var nwsHourly = SharedSession.NwsData.SelectedDailyHourlyForecasts[i];
        //    var weatherHourly = SharedSession.WeatherForecasts.Where(f => f.Time == nwsHourly.StartTime.Time).SingleOrDefault().DailyForecasts.SelectedDailyHourlyForecasts.Where(d => d.HourlyTime == nwsHourly.StartTime).SingleOrDefault<HourlyForecast>();
        //    var hourlies = new MappedHourly
        //    {
        //        StartTime = nwsHourly.StartTime,
        //        EndTime = nwsHourly.EndTime,
        //        Temperature = new TemperatureValue { Value = Math.Round((nwsHourly.Temperature + weatherHourly.Temperature2m.Value) / 2, 0) },
        //        SkHourlyAnimation = nwsHourly.SkHourlyAnimation,
        //        BriefForecast = nwsHourly.ShortForecast,
        //        GustsStatement = _forecastService.GustsStatement(nwsHourly.Supplemental.WindGust.Value ?? weatherHourly.WindGusts10m.Value)

        //        // TODO run the parts of the forecast through the feelslikeEngine, WeatherReporter and put strings together.
        //        // Look for repetition. if there is, start and end hours, if there isn't, commas between brief forecasts.





        //    });
        //return;
        //  }

        //Shared Helper
        private void ResetCancellation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }
        public void Debounce(Func<Task> action, TimeSpan delay)
        {
            // Cancel any pending execution
            _cts?.Cancel();
            _cts?.Dispose();

            // Create a new CTS for the new debounce cycle
            _cts = new CancellationTokenSource();
            var token = _cts.Token;


            // Run the delayed task
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, token); // Wait for inactivity
                    if (!token.IsCancellationRequested)
                    {
                        await action();
                    }
                }
                catch (TaskCanceledException e)
                {
                    Debug.WriteLine(e);
                    return;

                    // Expected when a new event occurs before delay ends
                }
            }, token);
        }
    }
}


//#1 by GPS
//private async Task<bool> AcquireLocationAsync()
//{
//    try
//    {
//        ProgressMessage = "Getting GPS location...";
//        var coordinates = await CurrentLocation.VerifyPermissionAsync();

//        if (coordinates == null)
//            return false;

//        SharedSession.SelectedLocation = new PhotoLocation
//        {
//            Latitude = coordinates.Value.Latitude,
//            Longitude = coordinates.Value.Longitude
//        };

//        Debug.WriteLine($"Latitude: {SharedSession.SelectedLocation.Latitude}");
//        Debug.WriteLine($"Longitude: {SharedSession.SelectedLocation.Longitude}");

//        ProgressMessage = "Setting Dates...";
//        try
//        {
//            await SetDates();
//            Debug.WriteLine("SUCCESSFULLY CREATED THE DATES ARRAY");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR CREATING THE DATES {ex}");
//        }
//        try
//        {
//            await FetchNwsWeatherAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE NWS COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE NWS COLLECTION {ex}");
//        }
//        try
//        {
//            //       await GetAstronomyEventsAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE ASTRONOMY COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE ASTRONOMY COLLECTION {ex}");
//        }
//        try
//        {
//            //   await GetWeatherDataAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE METEO COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE METEO COLLECTION {ex}");
//        }
//        try
//        {
//            await FetchMoonPhasesAsync();
//            Debug.WriteLine("SUCCESSFULLY BUILT THE MOON COLLECTION");
//        }
//        catch (Exception ex)
//        {
//            Debug.WriteLine($"ERROR BUILDING THE MOON COLLECTION {ex}");
//        }

//        IsWorking = false;
//        // Use absolute routing to avoid relative routing errors
//        await Shell.Current.GoToAsync("//Forecast");
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error acquiring GPS: {ex}");
//        return false;
//    }

//    return true;

//}
//#1 by User Input Location
//private async Task SetSelectedSuggestion(PhotoLocation selectedLocation)
//{

//    try
//    {
//        ProgressMessage = "Setting your selected location...";
//        //#1
//        SharedSession.SelectedLocation = selectedLocation;
//        DisplayName = selectedLocation.DisplayName;
//        if (Shell.Current.FindByName("InputLocation") is Entry location)
//        {
//            location.Text = DisplayName;
//            location.IsReadOnly = true;
//        }

//        ProgressMessage = "Setting Dates...";
//        //#3
//        await SetDates();

//        if (SharedSession.Dates.Length > 0)
//        {
//            Completed += 1;
//        }


//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error acquiring GPS: {ex}");
//        return;


//    }
//    return;
//}

//#4 Get Nws Weather
//private async Task<NwsWeatherModel> FetchNwsWeatherAsync()
//{
//    try
//    {
//        MainThread.BeginInvokeOnMainThread(async () =>
//        {
//            ProgressMessage = "Fetching weather...";

//            var result = await _nwsService.GetWeatherAsync(
//                SharedSession.SelectedLocation.Latitude,
//                SharedSession.SelectedLocation.Longitude);
//            if (result != null)
//            {
//                SharedSession.NwsData = result;
//                Debug.WriteLine($" added.");



//            }
//        });
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"Error fetching weather: {ex}");
//        return default;
//    }

//    return default;
//}
////#7 Get Moon data
//private async Task<FutureMoonPhase[]> FetchMoonPhasesAsync()
//{
//    ProgressMessage = "Fetching moon phases...";

//    var moonResults = await _moonService.GetFutureMoonPhasesAsync(SharedSession.StartDate, SharedSession.SelectedTimeZoneOffset);

//    if (moonResults != null)
//    {
//        Debug.WriteLine($"Moon phases count: {moonResults.Length}");
//        return moonResults;
//    }
//    return default;
//}
//#6 Get Open-Meteo Data
//private async Task<(ObservableCollection<WeatherForecasts>, ObservableCollection<DayViewModel>)> FetchDailyWeatherAsync()
//{
//    //try
//    //{
//    await MainThread.InvokeOnMainThreadAsync(async () =>
//      {
//          ProgressMessage = $"Calling the station in {SharedSession.SelectedLocation.City} for the weather...";
//          var result = await _weatherService.BuildWeatherForecastsAsync(eventResults.Item1,eventResults.Item2);
//          if (result.Item1 != null)
//          {
//              SharedSession.WeatherForecasts = new ObservableCollection<WeatherForecasts>(result.Item1);
//              Debug.WriteLine($"WeatherForecasts count {SharedSession.WeatherForecasts.Count}");
//          }
//          if (result.Item2 != null)
//          {
//              SharedSession.Days = new ObservableCollection<DayViewModel>(result.Item2);
//              Debug.WriteLine($"Days count {SharedSession.Days.Count}");
//          }
//          return result;
//      });
//    return default;
//}


//private async Task<ObservableCollection<DateItem>> SetDates()
//{
//    int nbrOfDays = EndDate.Time.Subtract(StartDate.Time).Days + 1;
//    var currentDate = StartDate.Time;
//    var dateItems = new ObservableCollection<DateItem>();
//    for (int i = 0; i < nbrOfDays; i++)
//    {

//        currentDate = currentDate.Time;

//        dateItems[i] = new DateItem
//        {
//            Time = currentDate
//        };
//        currentDate = currentDate.Time.AddDays(1);
//    }
//    return dateItems;
//}
//#5 Get Astronomy Events
//private async Task<ObservableCollection<EventModel>> GetAstronomyEventsAsync()
//{
//    try
//    {
//        ProgressMessage = "Lining up the Sun, the Moon, and stars...";

//        if (SharedSession.Dates != null && SharedSession.Dates.Length > 0)
//        {

//            var result = await _eventService.GetDayEventsAsync();
//            return result;

//        }
//        return default;
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"ERROR GETTING ASTRONOMY DATA {ex}");
//        return default;
//    }
//}

//#6: Call Weather Service
//private async Task<(ObservableCollection<WeatherForecasts>, ObservableCollection<DayViewModel>)> GetWeatherDataAsync()
//{

//    ProgressMessage = $"Calling the station in {SharedSession.SelectedLocation.City} for the weather...";

//    try
//    {

//        var result = await _weatherService.BuildWeatherForecastsAsync();
//        return result;
//    }
//    catch (Exception ex)
//    {
//        Debug.WriteLine($"[SelectLocationVM] Error getting weather data. Exception: {ex.GetType().Name}: {ex.Message}");
//        Debug.WriteLine($"[SelectLocationVM] StackTrace: {ex.StackTrace}");
//        ProgressMessage = $"Error: {ex.Message}";
//        return default;
//    }
//}


